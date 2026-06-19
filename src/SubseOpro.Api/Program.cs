using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SubseOpro.Api.Middleware;
using SubseOpro.Api.Services;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure;
using SubseOpro.Infrastructure.Repositories;
using SubseOpro.Infrastructure.Persistence;
using SubseOpro.Infrastructure.Persistence;
using SubseOpro.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, "SubseOpro.Api.xml");
    if (File.Exists(apiXmlPath))
    {
        options.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);
    }

    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, "SubseOpro.Application.xml");
    if (File.Exists(applicationXmlPath))
    {
        options.IncludeXmlComments(applicationXmlPath);
    }
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ISubsectionRepository, SubsectionRepository>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<HubContentService>();
builder.Services.AddSingleton<AiTokenEncryptionService>();
builder.Services.AddHttpClient();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret não configurado.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        };
    });

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy("Backoffice", policy => policy.RequireRole("admin", "moderator", "financeiro", "comunicacao", "secretaria"))
    .AddPolicy("PlatformAdmin", policy => policy.RequireRole("platform_admin"));

builder.Services.AddCors(options =>
{
    var configuredOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>();

    var defaultOrigins = CorsDefaults.Origins;

    options.AddPolicy("Frontend", policy =>
    {
        if (configuredOrigins is { Length: > 0 })
        {
            policy.WithOrigins(configuredOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy.SetIsOriginAllowed(origin =>
                {
                    if (CorsDefaults.Origins.Contains(origin)) return true;
                    if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    {
                        var host = uri.Host;
                        return host.EndsWith(".subse-pro.omnora.tech", StringComparison.OrdinalIgnoreCase)
                            || host.Equals("subse-pro.omnora.tech", StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                })
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseForwardedHeaders();
app.UseCors("Frontend");
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseMiddleware<PersonalAccessTokenMiddleware>();
app.UseMiddleware<TenantFromJwtMiddleware>();
app.UseMiddleware<TenantJwtValidationMiddleware>();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (builder.Configuration.GetValue<bool>("Seed:RunOnStartup"))
    {
        var seeder = scope.ServiceProvider.GetRequiredService<MockDataSeeder>();
        await seeder.SeedAsync();
    }
}

app.Run();

public partial class Program { }

internal static class CorsDefaults
{
    public static readonly string[] Origins =
    [
        "http://localhost:5173",
        "http://localhost:5174",
        "http://localhost:3000",
        "http://127.0.0.1:5173",
        "http://127.0.0.1:5174",
        "http://localhost:8081",
        "http://127.0.0.1:8081",
        "http://localhost:8082",
        "http://127.0.0.1:8082",
    ];
}
