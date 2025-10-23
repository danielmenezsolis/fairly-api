using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FairlyApi.Models
{
    [Table("users", Schema = "fairly")]
    public class Users
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required(ErrorMessage ="Name Required")]
        [MaxLength(100)]
        [Column("name"),]
        public string Name { get; set; }

        [Required(ErrorMessage ="email required")]
        [MaxLength(100)]
        [Column("email")]
        public string Email { get; set; }

        [MaxLength(255)]
        [Column("profile_picture_url")]
        public string ProfilePictureUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Group> CreatedGroups { get; set; } = new List<Group>();
        public virtual ICollection<GroupMember> GroupMembership { get; set; } = new List<GroupMember>();
        public virtual ICollection<Expense> PaidExpenses { get; set; } = new List<Expense>();
        public virtual ICollection<ExpenseParticipant> ExpenseParticipations { get; set; } = new List<ExpenseParticipant>();

    }
}
