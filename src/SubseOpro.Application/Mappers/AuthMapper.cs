using SubseOpro.Application.ViewModels.Auth;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class AuthMapper
{
    public static AuthUserViewModel EnrichWithSubsection(AuthUserViewModel user, Subsection subsection)
    {
        user.SubsectionId = subsection.Id.ToString();
        user.SubsectionSlug = subsection.Slug;
        user.SubsectionName = subsection.Name;
        user.PrimaryColor = subsection.PrimaryColor;
        user.AccentColor = subsection.AccentColor;
        user.LogoUrl = subsection.LogoUrl;
        user.HubAccessMode = subsection.HubAccessMode.ToString();
        return user;
    }
}
