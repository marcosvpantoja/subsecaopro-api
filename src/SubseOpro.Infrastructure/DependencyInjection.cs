using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;
using SubseOpro.Infrastructure.Repositories;
using SubseOpro.Infrastructure.Security;
using SubseOpro.Infrastructure.Seed;
using SubseOpro.Application.Services;
using SubseOpro.Infrastructure.Email;

namespace SubseOpro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dataSource));

        services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        services.AddScoped<ISmtpConfigurationRepository, SmtpConfigurationRepository>();
        services.AddScoped<IAdminWelcomeEmailService, AdminWelcomeEmailService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<MockDataSeeder>();

        return services;
    }
}
