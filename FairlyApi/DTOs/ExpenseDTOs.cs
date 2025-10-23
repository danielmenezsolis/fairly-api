namespace FairlyApi.DTOs
{
    public class CreateExpenseDto
    {
        public int GroupId { get; set; }
        public Guid PayerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateOnly ExpenseDate { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new();
    }

    // DTO para crear un gasto con división personalizada
    public class CreateCustomExpenseDto
    {
        public int GroupId { get; set; }
        public Guid PayerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateOnly ExpenseDate { get; set; }
        public List<ParticipantSplit> Participants { get; set; } = new();
    }

    // DTO para especificar cuánto debe cada participante
    public class ParticipantSplit
    {
        public Guid UserId { get; set; }
        public decimal AmountOwed { get; set; }
    }

    // DTO para el balance de un usuario en un grupo
    public class UserBalanceDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Balance { get; set; } // Positivo = le deben, Negativo = debe
    }

    // DTO para las transacciones de liquidación
    public class SettlementDto
    {
        public Guid FromUserId { get; set; }
        public string FromUserName { get; set; } = string.Empty;
        public Guid ToUserId { get; set; }
        public string ToUserName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    // DTO para el resumen de balances de un grupo
    public class GroupBalanceSummaryDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public List<UserBalanceDto> UserBalances { get; set; } = new();
        public List<SettlementDto> SuggestedSettlements { get; set; } = new();
    }
}
