using SubseOpro.Application.ViewModels.Banner;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class BannerMapper
{
    public static BannerViewModel ToViewModel(Banner entity)
    {
        return new BannerViewModel
        {
            Id = entity.Id.ToString(),
            Title = entity.Title,
            Subtitle = entity.Subtitle,
            Cta = entity.Cta,
            Color = entity.Color,
            Photo = entity.PhotoUrl,
            To = entity.LinkTo,
            Params = entity.LinkParams,
        };
    }

    public static List<BannerViewModel> ToViewModelList(IEnumerable<Banner> entities)
        => entities.Select(ToViewModel).ToList();
}
