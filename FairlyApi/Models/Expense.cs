using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FairlyApi.Models;

[Table("expenses", Schema = "fairly")]
public class Expense
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("group_id")]
    public int GroupId { get; set; }

    [Required]
    [Column("payer_id")]
    public Guid PayerId { get; set; }

    [Required]
    [Column("total_amount", TypeName = "numeric(10,2)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("expense_date")]
    public DateOnly ExpenseDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("GroupId")]
    public virtual Group Group { get; set; } = null!;

    [ForeignKey("PayerId")]
    public virtual Users Payer { get; set; } = null!;

    public virtual ICollection<ExpenseParticipant> Participants { get; set; } = new List<ExpenseParticipant>();
}