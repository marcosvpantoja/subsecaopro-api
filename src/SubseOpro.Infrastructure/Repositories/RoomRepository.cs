using Microsoft.EntityFrameworkCore;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Repositories;

public class RoomRepository : Repository<Room>, IRoomRepository
{
    private static readonly TimeOnly[] AllSlots =
    [
        new(8, 0), new(9, 0), new(10, 0), new(11, 0),
        new(14, 0), new(15, 0), new(16, 0), new(17, 0),
    ];

    public RoomRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Room>> GetAvailableAsync(CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetAvailableHoursAsync(
        Guid roomId, DateOnly date, CancellationToken ct = default)
    {
        var reservations = await Context.Reservations
            .AsNoTracking()
            .Where(r => r.RoomId == roomId && r.Date == date && r.Status != ReservationStatus.Cancelled)
            .Select(r => new { r.StartTime, r.EndTime })
            .ToListAsync(ct);

        return AllSlots
            .Where(slot =>
            {
                var slotEnd = slot.AddHours(2);
                return !reservations.Any(r => r.StartTime < slotEnd && r.EndTime > slot);
            })
            .Select(t => t.ToString("HH:mm"))
            .ToList();
    }
}
