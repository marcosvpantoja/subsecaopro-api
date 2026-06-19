using System.Globalization;
using SubseOpro.Application.ViewModels.Reservation;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class ReservationMapper
{
    public static ReservationViewModel ToViewModel(Reservation entity)
    {
        return new ReservationViewModel
        {
            Id = entity.Id.ToString(),
            LawyerName = entity.Lawyer?.Name ?? string.Empty,
            RoomName = entity.Room?.Name ?? string.Empty,
            Date = entity.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            Time = $"{entity.StartTime:HH}h–{entity.EndTime:HH}h",
            StartTime = entity.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            EndTime = entity.EndTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            Status = entity.Status.ToString().ToLowerInvariant(),
            Amount = entity.Amount,
        };
    }

    public static List<ReservationViewModel> ToViewModelList(IEnumerable<Reservation> entities)
        => entities.Select(ToViewModel).ToList();
}
