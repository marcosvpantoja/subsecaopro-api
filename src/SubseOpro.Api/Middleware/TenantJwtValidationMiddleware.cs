using System.Security.Claims;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Middleware;

public class TenantJwtValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TenantJwtValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true && !tenantContext.IsPlatformScope)
        {
            var accountType = context.User.FindFirstValue("account_type");
            if (accountType is "platform_admin")
            {
                await _next(context);
                return;
            }

            if (tenantContext.SubsectionId.HasValue &&
                accountType is "lawyer" or "backoffice")
            {
                var claimSubsection = context.User.FindFirstValue("subsection_id");
                if (string.IsNullOrEmpty(claimSubsection) ||
                    !Guid.TryParse(claimSubsection, out var tokenSubsectionId) ||
                    tokenSubsectionId != tenantContext.SubsectionId.Value)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { detail = "Token não pertence a esta subseção." });
                    return;
                }
            }
        }

        await _next(context);
    }
}
