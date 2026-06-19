using SubseOpro.Domain.Enums;

namespace SubseOpro.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public Guid LawyerId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ReservationStatus Status { get; set; }
    public decimal Amount { get; set; }

    public Lawyer Lawyer { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
