using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GabsHybridApp.Shared.Models;

public class Notification
{
    public int Id { get; set; }
    [StringLength(255)]
    [Required]
    public string? Title { get; set; }
    public string? Content { get; set; }
    public NotificationType NotificationType { get; set; } = NotificationType.Default;
    public string? NavigateToUrl { get; set; }
    public DateTime CreatedOn { get; set; }

    [ForeignKey("UserId")]
    public UserAccount? User { get; set; }
    public Guid? UserId { get; set; }

}

public enum NotificationType
{
    Default = 0,
    Info = 4,
    Success = 5,
    Warning = 6,
    Error = 7
}