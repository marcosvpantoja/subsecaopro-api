namespace SubseOpro.Application.ViewModels.Platform;

public class PlatformHubSettingsViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public string HubTitle { get; set; } = string.Empty;
    public string HubSubtitle { get; set; } = string.Empty;
    public string LoginBadge { get; set; } = string.Empty;
    public string LoginHeadline { get; set; } = string.Empty;
    public string LoginHeadlineAccent { get; set; } = string.Empty;
    public string LoginDescription { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1B3A6B";
    public string? AccentColor { get; set; }
    public string FooterText { get; set; } = string.Empty;
}

public class UpdatePlatformHubSettingsViewModel
{
    public string? ProductName { get; set; }
    public string? HubTitle { get; set; }
    public string? HubSubtitle { get; set; }
    public string? LoginBadge { get; set; }
    public string? LoginHeadline { get; set; }
    public string? LoginHeadlineAccent { get; set; }
    public string? LoginDescription { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? FooterText { get; set; }
}
