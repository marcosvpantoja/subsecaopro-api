namespace SubseOpro.Domain.Entities;

using SubseOpro.Domain.Enums;

public class Subsection
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? LogoUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1B3A6B";
    public string? AccentColor { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Hours { get; set; } = string.Empty;
    public bool IaEnabled { get; set; } = true;
    public string? IaSystemPrompt { get; set; }
    public string IaDisclaimer { get; set; } = string.Empty;
    public int IaMaxTurns { get; set; } = 8;
    public string IaAreasCobertasJson { get; set; } = "[]";
    public HubAccessMode HubAccessMode { get; set; } = HubAccessMode.Shared;
    public MobileAccessMode MobileAccessMode { get; set; } = MobileAccessMode.SharedApp;
    public string? DedicatedHubHost { get; set; }
    public string? EasProfileName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ActiveAiTokenEncrypted { get; set; }
    public Guid? ActiveAiTokenId { get; set; }
    public string? AiServiceUrl { get; set; }
}
