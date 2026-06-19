using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Infrastructure.Persistence;

/// <summary>
/// Tenant context used at design-time (migrations) — no filtering applied.
/// </summary>
public sealed class DesignTimeTenantContext : ITenantContext
{
    public Guid? SubsectionId => null;
    public string? Slug => null;
    public bool IsPlatformScope => false;
    public void SetTenant(Guid subsectionId, string slug) { }
    public void SetPlatformScope() { }
    public void Clear() { }
}
