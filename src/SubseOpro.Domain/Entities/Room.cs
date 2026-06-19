using SubseOpro.Domain.Enums;

namespace SubseOpro.Domain.Entities;

public class Room
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = [];
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsFree { get; set; }
    public RoomStatus Status { get; set; }
    public string Color { get; set; } = string.Empty;

    public ICollection<Reservation> Reservations { get; set; } = [];
}
