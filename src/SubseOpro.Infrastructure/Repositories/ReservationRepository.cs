using Microsoft.EntityFrameworkCore;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Repositories;

public class ReservationRepository : Repository<Reservation>, IReservationRepository
{
    public ReservationRepository(AppDbContext context) : base(context) { }

    public override async Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Room)
            .Include(r => r.Lawyer)
            .AsNoTracking()
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Reservation>> GetByLawyerIdAsync(Guid lawyerId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Room)
            .AsNoTracking()
            .Where(r => r.LawyerId == lawyerId)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Reservation>> GetPaidByLawyerIdAsync(Guid lawyerId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(r => r.Room)
            .AsNoTracking()
            .Where(r => r.LawyerId == lawyerId && r.Amount > 0)
            .OrderByDescending(r => r.Date)
            .ToListAsync(ct);
    }

    public async Task<Reservation> CreateReservationAsync(
        Guid lawyerId, Guid roomId, DateOnly date, TimeOnly start, TimeOnly end, CancellationToken ct = default)
    {
        var room = await Context.Rooms.FirstOrDefaultAsync(r => r.Id == roomId, ct)
            ?? throw new InvalidOperationException("Sala não encontrada.");

        if (room.Status == RoomStatus.Request)
            throw new InvalidOperationException("Esta sala exige solicitação via secretaria.");

        if (await HasConflictAsync(roomId, date, start, end, ct))
            throw new InvalidOperationException("Horário indisponível para esta sala.");

        var hours = (end - start).TotalHours;
        var amount = room.IsFree ? 0m : Math.Round((decimal)hours * room.PricePerHour * 1.04m, 2);

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            SubsectionId = room.SubsectionId,
            LawyerId = lawyerId,
            RoomId = roomId,
            Date = date,
            StartTime = start,
            EndTime = end,
            Status = ReservationStatus.Pending,
            Amount = amount,
        };

        await DbSet.AddAsync(reservation, ct);
        await Context.SaveChangesAsync(ct);

        reservation.Room = room;
        return reservation;
    }

    public async Task<Reservation> CancelReservationAsync(Guid reservationId, Guid lawyerId, CancellationToken ct = default)
    {
        var reservation = await DbSet
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.LawyerId == lawyerId, ct)
            ?? throw new InvalidOperationException("Reserva não encontrada.");

        if (reservation.Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("Reserva já cancelada.");

        reservation.Status = ReservationStatus.Cancelled;
        await Context.SaveChangesAsync(ct);
        return reservation;
    }

    public async Task<bool> HasConflictAsync(
        Guid roomId, DateOnly date, TimeOnly start, TimeOnly end, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(r =>
            r.RoomId == roomId &&
            r.Date == date &&
            r.Status != ReservationStatus.Cancelled &&
            r.StartTime < end &&
            r.EndTime > start, ct);
    }
}
