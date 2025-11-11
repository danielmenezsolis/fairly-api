using FairlyApi.Data;
using FairlyApi.DTOs;
using FairlyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace FairlyApi.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly FairlyDbContext _context;

        public ExpensesController(FairlyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {

            return await _context.Expenses
                    .Include(e => e.Payer)
                    .Include(e => e.Group)
                    .Include(e => e.Participants)
                        .ThenInclude(p => p.User)
                    .OrderByDescending(e => e.ExpenseDate)
                    .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            var expense = await _context.Expenses
                .Include(e => e.Payer)
                .Include(e => e.Group)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
            {
                return NotFound(new { message = "Gasto no encontrado" });
            }

            return expense;
        }

        [HttpPost("equal-split")]
        public async Task<ActionResult<Expense>> CreateExpenseEqualSplit(CreateExpenseDto expenseDto)
        {
            
                // Validaciones
                if (expenseDto.TotalAmount <= 0)
                {
                    return BadRequest(new { message = "El monto total debe ser mayor a 0" });
                }
                if (expenseDto.ParticipantIds == null || !expenseDto.ParticipantIds.Any())
                {
                    return BadRequest(new { message = "Debe haber al menos un participante" });
                }
             var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == expenseDto.GroupId);
            if (group == null)
            {
                return NotFound(new { message = "Grupo no encontrado" });
            }
           var payerIsMember = await _context.GroupMembers.AsNoTracking()
            .AnyAsync(gm => gm.GroupId == expenseDto.GroupId && gm.UserId == expenseDto.PayerId);
        
            if (!payerIsMember)
            {
                return BadRequest(new { message = "El pagador debe ser miembro del grupo" });
            }
            var memberIds = await _context.GroupMembers.AsNoTracking()
             .Where(gm => gm.GroupId == expenseDto.GroupId)
             .Select(gm => gm.UserId)
             .ToListAsync();
            
             var invalidParticipants = expenseDto.ParticipantIds.Except(memberIds).ToList();
            if (invalidParticipants.Any())
            {
                return BadRequest(new { message = "Todos los participantes deben ser miembros del grupo" });
            }
            // Crear el gasto
            var expense = new Expense
            {
                GroupId = expenseDto.GroupId,
                PayerId = expenseDto.PayerId,
                TotalAmount = expenseDto.TotalAmount,
                Description = expenseDto.Description,
                ExpenseDate = expenseDto.ExpenseDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            // Calcular división equitativa
            var amountPerPerson = Math.Round(expenseDto.TotalAmount / expenseDto.ParticipantIds.Count, 2);
            var totalAssigned = amountPerPerson * expenseDto.ParticipantIds.Count;
            var difference = expenseDto.TotalAmount - totalAssigned;

            // Crear participantes
            for (int i = 0; i < expenseDto.ParticipantIds.Count; i++)
            {
                var amount = amountPerPerson;

                // Ajustar el último participante para que cuadren los centavos
                if (i == expenseDto.ParticipantIds.Count - 1 && difference != 0)
                {
                    amount += difference;
                }

                var participant = new ExpenseParticipant
                {
                    ExpenseId = expense.Id,
                    UserId = expenseDto.ParticipantIds[i],
                    AmountOwed = amount
                };

                _context.ExpensesParticipant.Add(participant);
            }

            await _context.SaveChangesAsync();
            var createdExpense = await _context.Expenses
           .Include(e => e.Payer)
           .Include(e => e.Group)
           .Include(e => e.Participants)
               .ThenInclude(p => p.User)
           .FirstOrDefaultAsync(e => e.Id == expense.Id);

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, createdExpense);
        }

        [HttpPost("custom-split")]
        public async Task<ActionResult<Expense>> CreateExpenseCustomSplit(CreateCustomExpenseDto dto)
        {
            // Validaciones
            if (dto.TotalAmount <= 0)
            {
                return BadRequest(new { message = "El monto total debe ser mayor a 0" });
            }

            if (dto.Participants == null || !dto.Participants.Any())
            {
                return BadRequest(new { message = "Debe haber al menos un participante" });
            }

            // Verificar que la suma de los montos coincida con el total
            var sumOfAmounts = dto.Participants.Sum(p => p.AmountOwed);
            if (Math.Abs(sumOfAmounts - dto.TotalAmount) > 0.01m) // Tolerancia de 1 centavo
            {
                return BadRequest(new { message = $"La suma de los montos ({sumOfAmounts}) no coincide con el total ({dto.TotalAmount})" });
            }

            // Verificar que el grupo existe
            var group = await _context.Groups.FindAsync(dto.GroupId);
            if (group == null)
            {
                return NotFound(new { message = "Grupo no encontrado" });
            }

            // Verificar que el pagador es miembro del grupo
            var payerIsMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == dto.GroupId && gm.UserId == dto.PayerId);

            if (!payerIsMember)
            {
                return BadRequest(new { message = "El pagador debe ser miembro del grupo" });
            }

            // Verificar que todos los participantes son miembros del grupo
            foreach (var participant in dto.Participants)
            {
                if (participant.AmountOwed <= 0)
                {
                    return BadRequest(new { message = "Todos los montos deben ser mayores a 0" });
                }

                var isMember = await _context.GroupMembers
                    .AnyAsync(gm => gm.GroupId == dto.GroupId && gm.UserId == participant.UserId);

                if (!isMember)
                {
                    return BadRequest(new { message = "Todos los participantes deben ser miembros del grupo" });
                }
            }

            // Crear el gasto
            var expense = new Expense
            {
                GroupId = dto.GroupId,
                PayerId = dto.PayerId,
                TotalAmount = dto.TotalAmount,
                Description = dto.Description,
                ExpenseDate = dto.ExpenseDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            // Crear participantes con montos personalizados
            foreach (var participantDto in dto.Participants)
            {
                var participant = new ExpenseParticipant
                {
                    ExpenseId = expense.Id,
                    UserId = participantDto.UserId,
                    AmountOwed = participantDto.AmountOwed
                };

                _context.ExpensesParticipant.Add(participant);
            }

            await _context.SaveChangesAsync();

            // Recargar el gasto con sus relaciones
            var createdExpense = await _context.Expenses
                .Include(e => e.Payer)
                .Include(e => e.Group)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.Id == expense.Id);

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, createdExpense);
        }


        [HttpGet("group/{groupId}/balances")]
        public async Task<ActionResult<GroupBalanceSummaryDto>> GetGroupBalances(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return NotFound(new { message = "Grupo no encontrado" });
            }

            
            var expenses = await _context.Expenses
                .Include(e => e.Participants)
                .Where(e => e.GroupId == groupId)
                .ToListAsync();

           
            var balances = new Dictionary<Guid, decimal>();

            
            foreach (var member in group.Members)
            {
                balances[member.UserId] = 0;
            }

            
            foreach (var expense in expenses)
            {
                // El pagador recibe crédito (balance positivo)
                if (balances.ContainsKey(expense.PayerId))
                {
                    balances[expense.PayerId] += expense.TotalAmount;
                }

                
                foreach (var participant in expense.Participants)
                {
                    if (balances.ContainsKey(participant.UserId))
                    {
                        balances[participant.UserId] -= participant.AmountOwed;
                    }
                }
            }

            
            var userBalances = balances.Select(kvp => new UserBalanceDto
            {
                UserId = kvp.Key,
                UserName = group.Members.First(m => m.UserId == kvp.Key).User.Name,
                Balance = kvp.Value
            }).OrderByDescending(b => b.Balance).ToList();

            
            var settlements = CalculateSettlements(userBalances);

            var summary = new GroupBalanceSummaryDto
            {
                GroupId = groupId,
                GroupName = group.Name,
                UserBalances = userBalances,
                SuggestedSettlements = settlements
            };

            return Ok(summary);
        }

        // Método privado para calcular las liquidaciones óptimas
        private List<SettlementDto> CalculateSettlements(List<UserBalanceDto> balances)
        {
            var settlements = new List<SettlementDto>();

            // Crear copias de los balances para no modificar los originales
            var creditors = balances
                .Where(b => b.Balance > 0.01m)
                .Select(b => new { b.UserId, b.UserName, Balance = b.Balance })
                .OrderByDescending(b => b.Balance)
                .ToList();

            var debtors = balances
                .Where(b => b.Balance < -0.01m)
                .Select(b => new { b.UserId, b.UserName, Balance = b.Balance })
                .OrderBy(b => b.Balance)
                .ToList();

            // Usar listas mutables para el algoritmo
            var creditorsList = creditors.Select(c => new { c.UserId, c.UserName, Balance = c.Balance }).ToList();
            var debtorsList = debtors.Select(d => new { d.UserId, d.UserName, Balance = d.Balance }).ToList();

            int creditorIndex = 0;
            int debtorIndex = 0;

            while (creditorIndex < creditorsList.Count && debtorIndex < debtorsList.Count)
            {
                var creditor = creditorsList[creditorIndex];
                var debtor = debtorsList[debtorIndex];

                var amount = Math.Min(creditor.Balance, Math.Abs(debtor.Balance));
                amount = Math.Round(amount, 2);

                settlements.Add(new SettlementDto
                {
                    FromUserId = debtor.UserId,
                    FromUserName = debtor.UserName,
                    ToUserId = creditor.UserId,
                    ToUserName = creditor.UserName,
                    Amount = amount
                });

                // Actualizar balances temporales
                creditorsList[creditorIndex] = new { creditor.UserId, creditor.UserName, Balance = creditor.Balance - amount };
                debtorsList[debtorIndex] = new { debtor.UserId, debtor.UserName, Balance = debtor.Balance + amount };

                if (creditorsList[creditorIndex].Balance < 0.01m)
                {
                    creditorIndex++;
                }

                if (Math.Abs(debtorsList[debtorIndex].Balance) < 0.01m)
                {
                    debtorIndex++;
                }
            }

            return settlements;
        }

        // PUT: api/Expenses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, Expense expense)
        {
            if (id != expense.Id)
            {
                return BadRequest(new { message = "El ID no coincide" });
            }

            var existingExpense = await _context.Expenses.FindAsync(id);
            if (existingExpense == null)
            {
                return NotFound(new { message = "Gasto no encontrado" });
            }

            existingExpense.Description = expense.Description;
            existingExpense.ExpenseDate = expense.ExpenseDate;

             try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Expenses.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Expenses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                var expense = await _context.Expenses.FindAsync(id);
                if (expense == null)
                {
                    return NotFound(new { message = "Gasto no encontrado" });
                }

                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}