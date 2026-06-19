using System.Security.Claims;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Middleware;

/// <summary>
/// Após autenticação, define o tenant a partir do claim subsection_id quando
/// nenhum slug foi resolvido por header/subdomínio.
/// </summary>
public class TenantFromJwtMiddleware
{
    private readonly RequestDelegate _next;

    public TenantFromJwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        ISubsectionRepository subsections)
    {
        if (tenantContext.IsPlatformScope)
        {
            await _next(context);
            return;
        }

        if (!tenantContext.SubsectionId.HasValue &&
            context.User.Identity?.IsAuthenticated == true)
        {
            var accountType = context.User.FindFirstValue("account_type");
            if (accountType is "lawyer" or "backoffice")
            {
                var claim = context.User.FindFirstValue("subsection_id");
                if (Guid.TryParse(claim, out var subsectionId))
                {
                    var subsection = await subsections.GetByIdAsync(subsectionId, context.RequestAborted);
                    if (subsection is not null && subsection.IsActive)
                        tenantContext.SetTenant(subsection.Id, subsection.Slug);
                }
            }
        }

        if (!RequiresTenant(context) || tenantContext.SubsectionId.HasValue || tenantContext.IsPlatformScope)
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            detail = "Subseção não informada. Use o header X-Subsection-Slug, acesse via subdomínio ou autentique-se.",
        });
    }

    private static bool RequiresTenant(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.Equals("/api/v1/health", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/api/v1/subsections/resolve", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.Equals("/api/v1/platform/auth/login", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/api/v1/platform", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/api/v1/hub", StringComparison.OrdinalIgnoreCase)) return false;
        return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }
}
