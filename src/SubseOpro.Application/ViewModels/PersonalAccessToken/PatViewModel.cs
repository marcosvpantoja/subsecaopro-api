namespace SubseOpro.Application.ViewModels.PersonalAccessToken;

public class PatViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid SubsectionId { get; set; }
    public string SubsectionSlug { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsExpired { get; set; }
    public bool IsActiveAiToken { get; set; }
}
