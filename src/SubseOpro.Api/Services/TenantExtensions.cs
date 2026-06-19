using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Services;

public static class TenantExtensions
{
    public static Guid RequireSubsectionId(this ITenantContext context)
    {
        if (!context.SubsectionId.HasValue)
            throw new InvalidOperationException("Subseção não resolvida para esta requisição.");

        return context.SubsectionId.Value;
    }
}
