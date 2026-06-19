using SubseOpro.Application.ViewModels.Room;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class RoomMapper
{
    public static RoomViewModel ToViewModel(Room entity)
    {
        return new RoomViewModel
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Description = entity.Description,
            Features = entity.Features,
            Capacity = entity.Capacity,
            PricePerHour = entity.PricePerHour,
            Free = entity.IsFree,
            Status = entity.Status.ToString().ToLowerInvariant(),
            Color = entity.Color,
        };
    }

    public static List<RoomViewModel> ToViewModelList(IEnumerable<Room> entities)
        => entities.Select(ToViewModel).ToList();
}
