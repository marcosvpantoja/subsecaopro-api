using System.Security.Claims;
using SubseOpro.Api.Services;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Middleware;

public class PersonalAccessTokenMiddleware
{
    private readonly RequestDelegate _next;

    public PersonalAccessTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IRepositoryFactory factory,
        ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();

        if (!authHeader.StartsWith("Bearer spro_", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var rawToken = authHeader["Bearer ".Length..];
        var tokenHash = TokenHasher.ComputeSha256Hex(rawToken);

        var pat = await factory.PersonalAccessTokens.GetByTokenHashAsync(tokenHash, context.RequestAborted);

        if (pat is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { detail = "Token inválido." });
            return;
        }

        if (pat.IsRevoked)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { detail = "Token revogado." });
            return;
        }

        if (pat.ExpiresAt.HasValue && pat.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { detail = "Token expirado." });
            return;
        }

        if (pat.Subsection is null || !pat.Subsection.IsActive)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { detail = "Subseção associada ao token está inativa." });
            return;
        }

        tenantContext.SetTenant(pat.SubsectionId, pat.Subsection.Slug);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, pat.Id.ToString()),
            new(ClaimTypes.Name, pat.Name),
            new("account_type", "pat"),
            new("subsection_id", pat.SubsectionId.ToString()),
            new(ClaimTypes.Role, "pat"),
        };

        var identity = new ClaimsIdentity(claims, "PersonalAccessToken");
        context.User = new ClaimsPrincipal(identity);

        _ = factory.PersonalAccessTokens.UpdateLastUsedAtAsync(pat.Id, DateTimeOffset.UtcNow, CancellationToken.None);

        await _next(context);
    }
}
