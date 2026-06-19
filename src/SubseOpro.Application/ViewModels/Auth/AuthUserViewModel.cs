namespace SubseOpro.Application.ViewModels.Auth;

public class AuthUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Oab { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Whatsapp { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string Initials { get; set; } = string.Empty;
    public string AccountType { get; set; } = "lawyer";
    public string[] Roles { get; set; } = [];
    public string? SubsectionId { get; set; }
    public string? SubsectionSlug { get; set; }
    public string? SubsectionName { get; set; }
    public string? PrimaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? HubAccessMode { get; set; }
}
