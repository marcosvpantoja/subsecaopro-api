namespace SubseOpro.Application.ViewModels.Notification;

public class NotificationViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public bool? ForLawyer { get; set; }
    public string Href { get; set; } = string.Empty;
}
