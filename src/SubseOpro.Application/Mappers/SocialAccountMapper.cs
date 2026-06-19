using SubseOpro.Application.ViewModels.Social;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class SocialAccountMapper
{
    public static SocialAccountViewModel ToViewModel(SocialAccount entity)
    {
        return new SocialAccountViewModel
        {
            Id = entity.Id.ToString(),
            Platform = entity.Platform,
            Handle = entity.Handle,
            ProfileUrl = entity.ProfileUrl,
            Connected = entity.Connected,
            FollowersCount = entity.FollowersCount,
            LastSync = entity.LastSync,
        };
    }

    public static List<SocialAccountViewModel> ToViewModelList(IEnumerable<SocialAccount> entities)
        => entities.Select(ToViewModel).ToList();
}
