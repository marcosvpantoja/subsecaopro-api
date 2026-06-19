using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Services;

public sealed class TenantContext : ITenantContext
{
    public Guid? SubsectionId { get; private set; }
    public string? Slug { get; private set; }
    public bool IsPlatformScope { get; private set; }

    public void SetTenant(Guid subsectionId, string slug)
    {
        SubsectionId = subsectionId;
        Slug = slug;
        IsPlatformScope = false;
    }

    public void SetPlatformScope()
    {
        SubsectionId = null;
        Slug = null;
        IsPlatformScope = true;
    }

    public void Clear()
    {
        SubsectionId = null;
        Slug = null;
        IsPlatformScope = false;
    }
}
