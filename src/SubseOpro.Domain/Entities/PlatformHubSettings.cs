namespace SubseOpro.Domain.Entities;

/// <summary>
/// Branding do Hub compartilhado (login em URL padrão, sem subdomínio de tenant).
/// Singleton — não confundir com subseções multi-tenant.
/// </summary>
public class PlatformHubSettings
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = "SubseçãoPRO";
    public string HubTitle { get; set; } = "Acesse o Hub";
    public string HubSubtitle { get; set; } = "Painel administrativo das subseções OAB";
    public string LoginBadge { get; set; } = "Plataforma SubseçãoPRO";
    public string LoginHeadline { get; set; } = "Gestão inteligente";
    public string LoginHeadlineAccent { get; set; } = "da subseção";
    public string LoginDescription { get; set; } =
        "Administre advogados, salas, reservas, parceiros e eleições em um único painel seguro e eficiente.";
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1B3A6B";
    public string? AccentColor { get; set; } = "#C9A84C";
    public string FooterText { get; set; } = "SubseçãoPRO · Plataforma multi-tenant";
}
