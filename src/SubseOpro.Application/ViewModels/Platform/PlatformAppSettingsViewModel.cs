namespace SubseOpro.Application.ViewModels.Platform;

public class HubModuleConfigViewModel
{
    public bool Enabled { get; set; }
    public List<string> SubsectionSlugs { get; set; } = [];
}

public class PublicModulesConfigViewModel
{
    public HubModuleConfigViewModel Lawyers { get; set; } = new() { Enabled = true };
    public HubModuleConfigViewModel Banners { get; set; } = new() { Enabled = true };
    public HubModuleConfigViewModel Posts { get; set; } = new() { Enabled = true };
    public HubModuleConfigViewModel Articles { get; set; } = new() { Enabled = true };
    public HubModuleConfigViewModel About { get; set; } = new() { Enabled = true };
    public HubModuleConfigViewModel Elections { get; set; } = new() { Enabled = true };
    public HubModuleConfigViewModel Notifications { get; set; } = new();
}

public class HubSubsectionRefViewModel
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PrimaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? LogoUrl { get; set; }
}

public class PlatformAppSettingsViewModel
{
    public bool PublicAppEnabled { get; set; }
    public string PlatformName { get; set; } = string.Empty;
    public string? DefaultSubsectionSlug { get; set; }
    public string AiMode { get; set; } = "mock";
    public string? AiGlobalPrompt { get; set; }
    public string? AiDisclaimer { get; set; }
    public PublicModulesConfigViewModel PublicModules { get; set; } = new();
    public List<HubSubsectionRefViewModel> Subsections { get; set; } = [];
}

public class UpdatePlatformAppSettingsViewModel
{
    public bool? PublicAppEnabled { get; set; }
    public string? PlatformName { get; set; }
    public string? DefaultSubsectionSlug { get; set; }
    public string? AiMode { get; set; }
    public string? AiGlobalPrompt { get; set; }
    public string? AiDisclaimer { get; set; }
    public PublicModulesConfigViewModel? PublicModules { get; set; }
}
