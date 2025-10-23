using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FairlyApi.Data;
using FairlyApi.Models;

namespace FairlyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly FairlyDbContext _context;

    public GroupController(FairlyDbContext context)
    {
        _context = context;
    }

    // GET: api/Groups
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Group>>> GetGroups()
    {
        return await _context.Groups
            .Include(g => g.Creator)
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .ToListAsync();
    }

    // GET: api/Groups/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Group>> GetGroup(int id)
    {
        var group = await _context.Groups
            .Include(g => g.Creator)
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound(new { message = "Grupo no encontrado" });
        }

        return group;
    }

    // GET: api/Groups/{id}/members
    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<Users>>> GetGroupMembers(int id)
    {
        var groupExists = await _context.Groups.AnyAsync(g => g.Id == id);
        if (!groupExists)
        {
            return NotFound(new { message = "Grupo no encontrado" });
        }

        var members = await _context.GroupMembers
            .Where(gm => gm.GroupId == id)
            .Include(gm => gm.User)
            .Select(gm => gm.User)
            .ToListAsync();

        return Ok(members);
    }

    // GET: api/Groups/{id}/expenses
    [HttpGet("{id}/expenses")]
    public async Task<ActionResult<IEnumerable<Expense>>> GetGroupExpenses(int id)
    {
        var groupExists = await _context.Groups.AnyAsync(g => g.Id == id);
        if (!groupExists)
        {
            return NotFound(new { message = "Grupo no encontrado" });
        }

        var expenses = await _context.Expenses
            .Where(e => e.GroupId == id)
            .Include(e => e.Payer)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();

        return Ok(expenses);
    }

    // POST: api/Groups
    [HttpPost]
    public async Task<ActionResult<Group>> CreateGroup(Group group)
    {
        // Verificar que el creador existe
        var creatorExists = await _context.Users.AnyAsync(u => u.Id == group.CreatorId);
        if (!creatorExists)
        {
            return BadRequest(new { message = "El creador no existe" });
        }

        group.CreatedAt = DateTime.UtcNow;
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Agregar automáticamente al creador como miembro del grupo
        var groupMember = new GroupMember
        {
            GroupId = group.Id,
            UserId = group.CreatorId
        };
        _context.GroupMembers.Add(groupMember);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }

    // POST: api/Groups/{id}/members
    [HttpPost("{id}/members")]
    public async Task<ActionResult> AddMemberToGroup(int id, [FromBody] Guid userId)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null)
        {
            return NotFound(new { message = "Grupo no encontrado" });
        }

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return BadRequest(new { message = "Usuario no encontrado" });
        }

        // Verificar si ya es miembro
        var alreadyMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == id && gm.UserId == userId);

        if (alreadyMember)
        {
            return BadRequest(new { message = "El usuario ya es miembro del grupo" });
        }

        var groupMember = new GroupMember
        {
            GroupId = id,
            UserId = userId
        };

        _context.GroupMembers.Add(groupMember);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Miembro agregado exitosamente" });
    }

    // DELETE: api/Groups/{id}/members/{userId}
    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMemberFromGroup(int id, Guid userId)
    {
        var groupMember = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == id && gm.UserId == userId);

        if (groupMember == null)
        {
            return NotFound(new { message = "Miembro no encontrado en el grupo" });
        }

        _context.GroupMembers.Remove(groupMember);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/Groups/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroup(int id, Group group)
    {
        if (id != group.Id)
        {
            return BadRequest(new { message = "El ID no coincide" });
        }

        var existingGroup = await _context.Groups.FindAsync(id);
        if (existingGroup == null)
        {
            return NotFound(new { message = "Grupo no encontrado" });
        }

        existingGroup.Name = group.Name;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Groups.AnyAsync(g => g.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Groups/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null)
        {
            return NotFound(new { message = "Grupo no encontrado" });
        }

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}