using SubseOpro.Application.ViewModels.Platform;
using SubseOpro.Domain;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class PlatformHubSettingsMapper
{
    public static PlatformHubSettingsViewModel ToViewModel(PlatformHubSettings entity) =>
        new()
        {
            ProductName = entity.ProductName,
            HubTitle = entity.HubTitle,
            HubSubtitle = entity.HubSubtitle,
            LoginBadge = entity.LoginBadge,
            LoginHeadline = entity.LoginHeadline,
            LoginHeadlineAccent = entity.LoginHeadlineAccent,
            LoginDescription = entity.LoginDescription,
            LogoUrl = entity.LogoUrl,
            PrimaryColor = entity.PrimaryColor,
            AccentColor = entity.AccentColor,
            FooterText = entity.FooterText,
        };

    public static void ApplyUpdate(PlatformHubSettings entity, UpdatePlatformHubSettingsViewModel request)
    {
        if (request.ProductName is not null) entity.ProductName = request.ProductName.Trim();
        if (request.HubTitle is not null) entity.HubTitle = request.HubTitle.Trim();
        if (request.HubSubtitle is not null) entity.HubSubtitle = request.HubSubtitle.Trim();
        if (request.LoginBadge is not null) entity.LoginBadge = request.LoginBadge.Trim();
        if (request.LoginHeadline is not null) entity.LoginHeadline = request.LoginHeadline.Trim();
        if (request.LoginHeadlineAccent is not null) entity.LoginHeadlineAccent = request.LoginHeadlineAccent.Trim();
        if (request.LoginDescription is not null) entity.LoginDescription = request.LoginDescription.Trim();
        if (request.LogoUrl is not null) entity.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
        if (request.PrimaryColor is not null) entity.PrimaryColor = request.PrimaryColor.Trim();
        if (request.AccentColor is not null) entity.AccentColor = string.IsNullOrWhiteSpace(request.AccentColor) ? null : request.AccentColor.Trim();
        if (request.FooterText is not null) entity.FooterText = request.FooterText.Trim();
    }

    public static PlatformHubSettings CreateDefault() =>
        new()
        {
            Id = PlatformDefaults.HubSettingsId,
            ProductName = "SubseçãoPRO",
            HubTitle = "Acesse o Hub",
            HubSubtitle = "Painel administrativo das subseções OAB",
            LoginBadge = "Plataforma SubseçãoPRO",
            LoginHeadline = "Gestão inteligente",
            LoginHeadlineAccent = "da subseção",
            LoginDescription =
                "Administre advogados, salas, reservas, parceiros e eleições em um único painel seguro e eficiente.",
            PrimaryColor = "#1B3A6B",
            AccentColor = "#C9A84C",
            FooterText = "SubseçãoPRO · Plataforma multi-tenant",
        };
}
