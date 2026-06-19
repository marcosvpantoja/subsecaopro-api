using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.ViewModels.Dashboard;
using SubseOpro.Domain.Enums;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Api.Controllers;

/// <summary>
/// Estatísticas agregadas do dashboard backoffice.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Backoffice")]
public class DashboardController : ControllerBase
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retorna KPIs operacionais da subseção autenticada.
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsViewModel>> GetStats(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var monthStart = new DateOnly(now.Year, now.Month, 1);
        var previousMonthStart = monthStart.AddMonths(-1);
        var sevenDaysAgo = now.AddDays(-7);
        var monthStartUtc = monthStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var activeLawyers = await _context.Lawyers
            .AsNoTracking()
            .CountAsync(l => l.IsActive, ct);

        var activeLawyersAddedThisMonth = await _context.Lawyers
            .AsNoTracking()
            .CountAsync(l => l.IsActive && l.CreatedAt >= monthStartUtc, ct);

        var roomRevenueThisMonth = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Status == ReservationStatus.Confirmed && r.Date >= monthStart)
            .SumAsync(r => r.Amount, ct);

        var roomRevenuePreviousMonth = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Status == ReservationStatus.Confirmed
                && r.Date >= previousMonthStart
                && r.Date < monthStart)
            .SumAsync(r => r.Amount, ct);

        var roomRevenueChangePercent = roomRevenuePreviousMonth == 0m
            ? roomRevenueThisMonth > 0 ? 100d : 0d
            : Math.Round((double)((roomRevenueThisMonth - roomRevenuePreviousMonth) / roomRevenuePreviousMonth * 100m), 1);

        var todayReservations = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Date == today)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Confirmed = g.Count(r => r.Status == ReservationStatus.Confirmed),
                Pending = g.Count(r => r.Status == ReservationStatus.Pending),
            })
            .FirstOrDefaultAsync(ct);

        var couponsGenerated = await _context.Partners.AsNoTracking().CountAsync(ct);

        var couponsGeneratedLast7Days = await _context.Partners
            .AsNoTracking()
            .CountAsync(p => p.CreatedAt >= sevenDaysAgo, ct);

        var revenueLast7DaysStart = today.AddDays(-6);
        var confirmedReservationsLast7Days = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Status == ReservationStatus.Confirmed
                && r.Date >= revenueLast7DaysStart
                && r.Date <= today)
            .Select(r => new { r.Date, r.Amount })
            .ToListAsync(ct);

        var revenueLast7Days = new List<DashboardRevenueDayViewModel>();
        for (var offset = 0; offset < 7; offset++)
        {
            var date = revenueLast7DaysStart.AddDays(offset);
            var amount = confirmedReservationsLast7Days
                .Where(r => r.Date == date)
                .Sum(r => r.Amount);

            revenueLast7Days.Add(new DashboardRevenueDayViewModel
            {
                Day = date.ToString("ddd", PtBr).Replace(".", "", StringComparison.Ordinal),
                Amount = amount,
            });
        }

        var revenueLast7DaysTotal = revenueLast7Days.Sum(d => d.Amount);

        return Ok(new DashboardStatsViewModel
        {
            ActiveLawyers = activeLawyers,
            ActiveLawyersAddedThisMonth = activeLawyersAddedThisMonth,
            RoomRevenueThisMonth = roomRevenueThisMonth,
            RoomRevenueChangePercent = roomRevenueChangePercent,
            ReservationsToday = todayReservations?.Total ?? 0,
            ReservationsTodayConfirmed = todayReservations?.Confirmed ?? 0,
            ReservationsTodayPending = todayReservations?.Pending ?? 0,
            CouponsGenerated = couponsGenerated,
            CouponsGeneratedLast7Days = couponsGeneratedLast7Days,
            RevenueLast7Days = revenueLast7Days,
            RevenueLast7DaysTotal = revenueLast7DaysTotal,
        });
    }
}
