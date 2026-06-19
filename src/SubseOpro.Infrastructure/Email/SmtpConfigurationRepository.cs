using System.Net;
using System.Net.Sockets;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using SubseOpro.Application.Services;
using SubseOpro.Domain.Entities;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Email;

public class SmtpConfigurationRepository : ISmtpConfigurationRepository
{
    private static readonly TimeSpan SmtpOperationTimeout = TimeSpan.FromSeconds(20);
    private readonly AppDbContext _db;

    public SmtpConfigurationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SmtpConfigurationDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var current = await _db.SmtpConfigurations.AsNoTracking()
            .OrderByDescending(s => s.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            return new SmtpConfigurationDto(
                "smtp.sendgrid.net",
                587,
                "apikey",
                string.Empty,
                "no-reply@subsecao.pro",
                "SubseçãoPRO",
                true,
                false);
        }

        return new SmtpConfigurationDto(
            current.Host,
            current.Port,
            current.Username,
            current.Password,
            current.FromEmail,
            current.FromName,
            current.UseTls,
            current.IsActive);
    }

    public async Task<SmtpSaveResult> SaveAsync(SaveSmtpConfigurationInput input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.Host) || input.Port <= 0 || string.IsNullOrWhiteSpace(input.FromEmail))
            throw new ArgumentException("host, port e fromEmail são obrigatórios.");

        var current = await _db.SmtpConfigurations
            .OrderByDescending(s => s.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            current = new SmtpConfiguration { Id = Guid.NewGuid() };
            _db.SmtpConfigurations.Add(current);
        }

        current.Host = input.Host.Trim();
        current.Port = input.Port;
        current.Username = input.Username?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(input.Password))
            current.Password = input.Password;
        current.FromEmail = input.FromEmail.Trim();
        current.FromName = input.FromName?.Trim() ?? string.Empty;
        current.UseTls = input.UseTls;
        current.IsActive = input.IsActive;
        current.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return new SmtpSaveResult(true, current.UpdatedAt);
    }

    public async Task<SmtpTestResult> SendTestAsync(SendSmtpTestInput input, CancellationToken cancellationToken = default)
    {
        var current = await GetConfiguredSmtpAsync(cancellationToken);
        var toEmail = string.IsNullOrWhiteSpace(input.ToEmail) ? current.FromEmail : input.ToEmail.Trim();

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(current.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "SubseçãoPRO — Teste SMTP";
        message.Body = new TextPart("plain")
        {
            Text = "SMTP está configurado corretamente na plataforma SubseçãoPRO."
        };

        await SendWithTimeoutAsync(current, async (client, ct) => await client.SendAsync(message, ct), cancellationToken);
        return new SmtpTestResult(true, toEmail);
    }

    public async Task<SmtpConnectionCheckResult> ValidateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var current = await GetConfiguredSmtpAsync(cancellationToken);
        await SendWithTimeoutAsync(current, static (_, _) => Task.CompletedTask, cancellationToken);
        return new SmtpConnectionCheckResult(true, current.Host, current.Port, current.UseTls);
    }

    public async Task<SmtpSendResult> SendAsync(SendSmtpEmailInput input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.ToEmail))
            throw new ArgumentException("toEmail é obrigatório.");
        if (string.IsNullOrWhiteSpace(input.Subject))
            throw new ArgumentException("subject é obrigatório.");
        if (string.IsNullOrWhiteSpace(input.Body))
            throw new ArgumentException("body é obrigatório.");

        var current = await GetConfiguredSmtpAsync(cancellationToken);
        var toEmail = input.ToEmail.Trim();
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(current.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = input.Subject.Trim();
        message.Body = input.IsHtml
            ? new TextPart("html") { Text = input.Body }
            : new TextPart("plain") { Text = input.Body };

        await SendWithTimeoutAsync(current, async (client, ct) => await client.SendAsync(message, ct), cancellationToken);
        return new SmtpSendResult(true, toEmail, message.Subject);
    }

    private async Task<SmtpConfiguration> GetConfiguredSmtpAsync(CancellationToken cancellationToken)
    {
        var current = await _db.SmtpConfigurations.AsNoTracking()
            .OrderByDescending(s => s.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
            throw new InvalidOperationException("Nenhuma configuração SMTP encontrada.");

        if (!current.IsActive)
            throw new InvalidOperationException("Configuração SMTP inativa.");

        if (string.IsNullOrWhiteSpace(current.Host) ||
            current.Port <= 0 ||
            string.IsNullOrWhiteSpace(current.Username) ||
            string.IsNullOrWhiteSpace(current.Password) ||
            string.IsNullOrWhiteSpace(current.FromEmail))
        {
            throw new InvalidOperationException("Configuração SMTP incompleta.");
        }

        return current;
    }

    private static SecureSocketOptions ResolveSecureOptions(int port, bool useTls)
    {
        if (port == 465)
            return SecureSocketOptions.SslOnConnect;
        return useTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
    }

    private async Task SendWithTimeoutAsync(
        SmtpConfiguration smtp,
        Func<SmtpClient, CancellationToken, Task> afterConnect,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SmtpOperationTimeout);
        try
        {
            using var client = new SmtpClient
            {
                Timeout = (int)SmtpOperationTimeout.TotalMilliseconds,
                CheckCertificateRevocation = false
            };

            var socketOptions = ResolveSecureOptions(smtp.Port, smtp.UseTls);
            await ConnectPreferIpv4Async(client, smtp.Host, smtp.Port, socketOptions, timeoutCts.Token);
            await client.AuthenticateAsync(new NetworkCredential(smtp.Username, smtp.Password), timeoutCts.Token);
            await afterConnect(client, timeoutCts.Token);

            if (client.IsConnected)
                await client.DisconnectAsync(true, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Operação SMTP expirou após {(int)SmtpOperationTimeout.TotalSeconds} segundos.");
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException($"Erro de socket SMTP ({ex.SocketErrorCode}): {ex.Message}");
        }
    }

    private static async Task ConnectPreferIpv4Async(
        SmtpClient client,
        string host,
        int port,
        SecureSocketOptions options,
        CancellationToken cancellationToken)
    {
        if (IPAddress.TryParse(host, out var parsedIp))
        {
            await ConnectOneAsync(client, host, port, options, parsedIp, cancellationToken);
            return;
        }

        var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
        var targetAddress = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
            ?? addresses.FirstOrDefault();
        if (targetAddress is null)
            throw new InvalidOperationException($"Não foi possível resolver o host SMTP '{host}'.");

        await ConnectOneAsync(client, host, port, options, targetAddress, cancellationToken);
    }

    private static async Task ConnectOneAsync(
        SmtpClient client,
        string host,
        int port,
        SecureSocketOptions options,
        IPAddress address,
        CancellationToken cancellationToken)
    {
        var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };

        try
        {
            await socket.ConnectAsync(new IPEndPoint(address, port), cancellationToken);
            await client.ConnectAsync(socket, host, port, options, cancellationToken);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}
