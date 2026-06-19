namespace SubseOpro.Domain.Entities;

public class NotificationRead
{
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; }
}
