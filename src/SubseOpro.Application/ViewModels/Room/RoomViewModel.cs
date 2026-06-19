namespace SubseOpro.Application.ViewModels.Room;

public class RoomViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = [];
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }
    public bool Free { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
