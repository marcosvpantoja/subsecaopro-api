namespace SubseOpro.Application.ViewModels.Subsection;

public class SubsectionInfoViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1B3A6B";
    public string? AccentColor { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Hours { get; set; } = string.Empty;
    public bool IaEnabled { get; set; } = true;
    public string IaSystemPrompt { get; set; } = string.Empty;
    public string IaDisclaimer { get; set; } = string.Empty;
    public int IaMaxTurns { get; set; } = 8;
    public IReadOnlyList<string> IaAreasCobertas { get; set; } = [];
    public IReadOnlyList<SubsectionSocialLinkViewModel> SocialLinks { get; set; } = [];
    public string HubAccessMode { get; set; } = "Shared";
    public string MobileAccessMode { get; set; } = "SharedApp";
    public string? DedicatedHubHost { get; set; }
    public string? EasProfileName { get; set; }
    public string? AiServiceUrl { get; set; }
    public int AdminCount { get; set; }
    public bool? WelcomeEmailSent { get; set; }
}

public class SubsectionSocialLinkViewModel
{
    public string Platform { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
    public string ProfileUrl { get; set; } = string.Empty;
}

public class SubsectionResolveViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1B3A6B";
    public string? AccentColor { get; set; }
    public string? ActiveAiTokenEncrypted { get; set; }
}

public class SubsectionListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int AdminCount { get; set; }
    public string HubAccessMode { get; set; } = "Shared";
    public string MobileAccessMode { get; set; } = "SharedApp";
    public bool HasDedicatedSubdomain { get; set; }
}

public class CreateSubsectionViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1B3A6B";
    public string? AccentColor { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Hours { get; set; } = string.Empty;
    public bool? IaEnabled { get; set; }
    public string? IaSystemPrompt { get; set; }
    public string? IaDisclaimer { get; set; }
    public int? IaMaxTurns { get; set; }
    public IReadOnlyList<string>? IaAreasCobertas { get; set; }
    public string? AdminName { get; set; }
    public string? AdminEmail { get; set; }
    public string? AdminPassword { get; set; }
    public string? HubAccessMode { get; set; }
    public string? MobileAccessMode { get; set; }
    public string? DedicatedHubHost { get; set; }
    public string? EasProfileName { get; set; }
}

public class UpdateSubsectionViewModel
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public bool? IsActive { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Hours { get; set; }
    public bool? IaEnabled { get; set; }
    public string? IaSystemPrompt { get; set; }
    public string? IaDisclaimer { get; set; }
    public int? IaMaxTurns { get; set; }
    public IReadOnlyList<string>? IaAreasCobertas { get; set; }
    public string? HubAccessMode { get; set; }
    public string? MobileAccessMode { get; set; }
    public string? DedicatedHubHost { get; set; }
    public string? EasProfileName { get; set; }
    public string? AiServiceUrl { get; set; }
}

/// <summary>Campos operacionais editáveis pelo backoffice da subseção (sem branding/slug).</summary>
public class UpdateSubsectionSettingsViewModel
{
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Hours { get; set; }
    public bool? IaEnabled { get; set; }
    public string? IaSystemPrompt { get; set; }
    public string? IaDisclaimer { get; set; }
    public int? IaMaxTurns { get; set; }
    public IReadOnlyList<string>? IaAreasCobertas { get; set; }
}
