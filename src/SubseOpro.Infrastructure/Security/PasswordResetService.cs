using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubseOpro.Application.Services;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Security;

public class PasswordResetService : IPasswordResetService
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromHours(1);

    private readonly AppDbContext _db;
    private readonly IRepositoryFactory _factory;
    private readonly ISmtpConfigurationRepository _smtp;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        AppDbContext db,
        IRepositoryFactory factory,
        ISmtpConfigurationRepository smtp,
        IPasswordHasher passwordHasher,
        ILogger<PasswordResetService> logger)
    {
        _db = db;
        _factory = factory;
        _smtp = smtp;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task RequestResetAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var admin = await _factory.AdminUsers.GetByEmailGlobalAsync(normalizedEmail, cancellationToken);
        if (admin is null || !admin.IsActive)
            return;

        var code = Random.Shared.Next(100_000, 999_999).ToString();

        var existing = await _db.PasswordResetCodes
            .Where(c => c.Email == normalizedEmail && !c.IsUsed)
            .ToListAsync(cancellationToken);
        _db.PasswordResetCodes.RemoveRange(existing);

        _db.PasswordResetCodes.Add(new PasswordResetCode
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Code = code,
            ExpiresAt = DateTime.UtcNow.Add(CodeLifetime),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _smtp.SendAsync(
                new SendSmtpEmailInput(
                    normalizedEmail,
                    "SubseçãoPRO — Código de recuperação",
                    BuildResetEmailBody(code),
                    true),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail de recuperação para {Email}", normalizedEmail);
        }
    }

    public async Task<bool> ResetPasswordAsync(
        string email,
        string code,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(newPassword))
        {
            return false;
        }

        if (!PasswordPolicy.IsValid(newPassword, out _))
            return false;

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedCode = code.Trim();

        if (normalizedCode.Length != 6 || !normalizedCode.All(char.IsDigit))
            return false;

        var resetCode = await _db.PasswordResetCodes
            .Where(c =>
                c.Email == normalizedEmail &&
                c.Code == normalizedCode &&
                !c.IsUsed &&
                c.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (resetCode is null)
            return false;

        var updated = await _factory.AdminUsers.UpdatePasswordHashByEmailAsync(
            normalizedEmail,
            _passwordHasher.Hash(newPassword),
            cancellationToken);

        if (!updated)
            return false;

        resetCode.IsUsed = true;
        _db.PasswordResetCodes.Update(resetCode);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string BuildResetEmailBody(string code)
    {
        var safeCode = System.Net.WebUtility.HtmlEncode(code);
        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 480px; margin: 0 auto;">
              <p style="color: #1B3A6B; font-size: 11px; letter-spacing: 3px; text-transform: uppercase;">SubseçãoPRO</p>
              <h2 style="margin: 0 0 8px;">Recuperação de acesso</h2>
              <p style="color: #666; margin: 0 0 24px;">Use o código abaixo para definir uma nova senha. Válido por 1 hora.</p>
              <div style="background: #f4f6fa; border: 1px solid #1B3A6B; border-radius: 8px; padding: 24px; text-align: center; margin-bottom: 24px;">
                <p style="color: #888; font-size: 11px; letter-spacing: 2px; margin: 0 0 8px;">CÓDIGO DE RECUPERAÇÃO</p>
                <p style="color: #1B3A6B; font-size: 36px; font-weight: 700; letter-spacing: 10px; margin: 0;">{safeCode}</p>
              </div>
              <p style="color: #888; font-size: 12px;">Se você não solicitou a recuperação de senha, ignore este e-mail.</p>
            </body>
            </html>
            """;
    }
}
