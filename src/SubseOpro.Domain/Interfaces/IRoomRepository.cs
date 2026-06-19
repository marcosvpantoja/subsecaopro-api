using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface IRoomRepository : IRepository<Room>
{
    Task<IReadOnlyList<Room>> GetAvailableAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetAvailableHoursAsync(Guid roomId, DateOnly date, CancellationToken ct = default);
}
