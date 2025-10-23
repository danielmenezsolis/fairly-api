
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FairlyApi.Models;

[Table("expenseparticipants", Schema = "fairly")]
public class ExpenseParticipant
{
    [Column("expense_id")]
    public int ExpenseId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [Column("amount_owed", TypeName = "numeric(10,2)")]
    public decimal AmountOwed { get; set; }

    // Navigation properties
    [ForeignKey("ExpenseId")]
    public virtual Expense Expense { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual Users User { get; set; } = null!;
}