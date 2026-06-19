using SubseOpro.Domain.Interfaces;



namespace SubseOpro.Api.Middleware;



public class TenantResolutionMiddleware

{

    private const string TenantHeader = "X-Subsection-Slug";

    private static readonly string[] PlatformHosts = ["admin", "platform"];



    private readonly RequestDelegate _next;



    public TenantResolutionMiddleware(RequestDelegate next)

    {

        _next = next;

    }



    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ISubsectionRepository subsections)

    {

        var path = context.Request.Path.Value ?? string.Empty;



        if (IsExemptPath(path))

        {

            await _next(context);

            return;

        }



        if (path.StartsWith("/api/v1/platform", StringComparison.OrdinalIgnoreCase))

        {

            tenantContext.SetPlatformScope();

            await _next(context);

            return;

        }



        var slug = ResolveSlug(context);

        if (string.IsNullOrWhiteSpace(slug))

        {

            await _next(context);

            return;

        }



        var subsection = await subsections.GetBySlugAsync(slug.Trim().ToLowerInvariant(), context.RequestAborted);

        if (subsection is null)

        {

            context.Response.StatusCode = StatusCodes.Status404NotFound;

            await context.Response.WriteAsJsonAsync(new { detail = "Subseção não encontrada." });

            return;

        }



        if (!subsection.IsActive)

        {

            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(new { detail = "Esta subseção está temporariamente indisponível." });

            return;

        }



        tenantContext.SetTenant(subsection.Id, subsection.Slug);

        await _next(context);

    }



    private static bool IsExemptPath(string path)

    {

        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)) return true;

        if (path.Equals("/api/v1/health", StringComparison.OrdinalIgnoreCase)) return true;

        if (path.StartsWith("/api/v1/subsections/resolve", StringComparison.OrdinalIgnoreCase)) return true;

        if (path.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase)) return true;

        if (path.Equals("/api/v1/platform/auth/login", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.StartsWith("/api/v1/hub", StringComparison.OrdinalIgnoreCase)) return true;
        return false;

    }



    private static string? ResolveSlug(HttpContext context)

    {

        if (context.Request.Headers.TryGetValue(TenantHeader, out var headerSlug) &&

            !string.IsNullOrWhiteSpace(headerSlug))

        {

            return headerSlug.ToString().Trim().ToLowerInvariant();

        }



        var host = context.Request.Host.Host;

        if (string.IsNullOrWhiteSpace(host)) return null;



        var parts = host.Split('.');

        if (parts.Length >= 3)

        {

            var subdomain = parts[0].ToLowerInvariant();

            if (!PlatformHosts.Contains(subdomain) && subdomain is not ("www" or "api" or "api-subse"))

                return subdomain;

        }



        return null;

    }

}

