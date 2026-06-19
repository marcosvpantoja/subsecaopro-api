namespace SubseOpro.Domain.Entities;

public class PersonalAccessToken
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public Guid SubsectionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public bool IsRevoked { get; set; }
    public Subsection Subsection { get; set; } = null!;
}
