namespace SubseOpro.Application.ViewModels.Reservation;

public class ReservationViewModel
{
    public string Id { get; set; } = string.Empty;
    public string LawyerName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
