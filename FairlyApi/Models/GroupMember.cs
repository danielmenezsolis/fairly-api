using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FairlyApi.Models;

[Table("groupmembers", Schema = "fairly")]
public class GroupMember
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("group_id")]
    public int GroupId { get; set; }

    [ForeignKey("UserId")]
    public virtual Users User { get; set; } = null!;

    [ForeignKey("GroupId")]
    public virtual Group Group { get; set; } = null!;
}