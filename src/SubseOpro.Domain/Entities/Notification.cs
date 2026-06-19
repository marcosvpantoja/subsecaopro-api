namespace SubseOpro.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public string Icon { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool ForLawyerOnly { get; set; }
    public string Href { get; set; } = string.Empty;

    public ICollection<NotificationRead> Reads { get; set; } = [];
}
