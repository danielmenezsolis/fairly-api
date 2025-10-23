using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FairlyApi.Models
{
    [Table("groups",Schema ="fairly")]
    public class Group
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("creator_id")]
        public Guid CreatorId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CreatorId")]
        public virtual Users? Creator { get; set; } = null!;

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
