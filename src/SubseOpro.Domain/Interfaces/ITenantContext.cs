namespace SubseOpro.Domain.Interfaces;

public interface ITenantContext
{
    Guid? SubsectionId { get; }
    string? Slug { get; }
    bool IsPlatformScope { get; }
    void SetTenant(Guid subsectionId, string slug);
    void SetPlatformScope();
    void Clear();
}
