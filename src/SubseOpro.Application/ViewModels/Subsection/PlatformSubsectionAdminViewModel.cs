namespace SubseOpro.Application.ViewModels.Subsection;

public class PlatformSubsectionAdminViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreatePlatformSubsectionAdminResultViewModel : PlatformSubsectionAdminViewModel
{
    public bool WelcomeEmailSent { get; set; }
}

public class CreatePlatformSubsectionAdminViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "admin";
    public bool IsActive { get; set; } = true;
}

public class UpdatePlatformSubsectionAdminViewModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? Password { get; set; }
}

public class SubsectionBootstrapResultViewModel
{
    public bool Created { get; set; }
    public int BannersCreated { get; set; }
    public int RoomsCreated { get; set; }
    public int SocialAccountsCreated { get; set; }
}

public class SubsectionDeployChecklistViewModel
{
    public string Slug { get; set; } = string.Empty;
    public string HubAccessMode { get; set; } = string.Empty;
    public string MobileAccessMode { get; set; } = string.Empty;
    public string? DedicatedHubHost { get; set; }
    public string? EasProfileName { get; set; }
    public bool SlugResolveOk { get; set; }
    public string? SlugResolveMessage { get; set; }
    public IReadOnlyList<string> DeploySteps { get; set; } = [];
}
