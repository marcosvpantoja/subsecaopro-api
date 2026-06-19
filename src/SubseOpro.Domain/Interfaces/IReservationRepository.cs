using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<IReadOnlyList<Reservation>> GetByLawyerIdAsync(Guid lawyerId, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetPaidByLawyerIdAsync(Guid lawyerId, CancellationToken ct = default);
    Task<Reservation> CreateReservationAsync(Guid lawyerId, Guid roomId, DateOnly date, TimeOnly start, TimeOnly end, CancellationToken ct = default);
    Task<Reservation> CancelReservationAsync(Guid reservationId, Guid lawyerId, CancellationToken ct = default);
    Task<bool> HasConflictAsync(Guid roomId, DateOnly date, TimeOnly start, TimeOnly end, CancellationToken ct = default);
}
