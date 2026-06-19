using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SubseOpro.Application.Services;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;

namespace SubseOpro.Infrastructure.Email;

public class AdminWelcomeEmailService : IAdminWelcomeEmailService
{
    private readonly ISmtpConfigurationRepository _smtp;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminWelcomeEmailService> _logger;

    public AdminWelcomeEmailService(
        ISmtpConfigurationRepository smtp,
        IConfiguration configuration,
        ILogger<AdminWelcomeEmailService> logger)
    {
        _smtp = smtp;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> TrySendWelcomeAsync(
        AdminUser admin,
        Subsection subsection,
        string plainPassword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var loginUrl = ResolveLoginUrl(subsection);
            var body = BuildWelcomeEmailBody(admin.Name, admin.Email, plainPassword, subsection.Name, loginUrl);
            await _smtp.SendAsync(
                new SendSmtpEmailInput(
                    admin.Email,
                    "SubseçãoPRO — Credenciais de acesso",
                    body,
                    true),
                cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail de boas-vindas para {Email}", admin.Email);
            return false;
        }
    }

    private string ResolveLoginUrl(Subsection subsection)
    {
        if (subsection.HubAccessMode == HubAccessMode.DedicatedSubdomain)
        {
            var host = subsection.DedicatedHubHost?.Trim()
                ?? $"{subsection.Slug}.subse-pro.omnora.tech";
            return $"https://{host.TrimStart('/')}/login".Replace("https://https://", "https://");
        }

        return _configuration["Platform:SharedHubLoginUrl"]
            ?? "https://subse-pro.omnora.tech/login";
    }

    private static string BuildWelcomeEmailBody(
        string adminName,
        string email,
        string password,
        string subsectionName,
        string loginUrl)
    {
        var safeName = System.Net.WebUtility.HtmlEncode(adminName);
        var safeEmail = System.Net.WebUtility.HtmlEncode(email);
        var safePassword = System.Net.WebUtility.HtmlEncode(password);
        var safeSubsection = System.Net.WebUtility.HtmlEncode(subsectionName);
        var safeLoginUrl = System.Net.WebUtility.HtmlEncode(loginUrl);

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
              <p>Olá, <strong>{safeName}</strong>,</p>
              <p>Você foi cadastrado como administrador da subseção <strong>{safeSubsection}</strong> na plataforma SubseçãoPRO.</p>
              <p>Use as credenciais abaixo para acessar o Hub web e o aplicativo mobile:</p>
              <table style="border-collapse: collapse; margin: 16px 0;">
                <tr><td style="padding: 4px 12px 4px 0;"><strong>E-mail:</strong></td><td>{safeEmail}</td></tr>
                <tr><td style="padding: 4px 12px 4px 0;"><strong>Senha:</strong></td><td>{safePassword}</td></tr>
              </table>
              <p><a href="{safeLoginUrl}" style="color: #1B3A6B;">{safeLoginUrl}</a></p>
              <p style="font-size: 13px; color: #666;">Recomendamos alterar sua senha após o primeiro acesso.</p>
              <p style="font-size: 13px; color: #666;">SubseçãoPRO · Plataforma multi-tenant</p>
            </body>
            </html>
            """;
    }
}
