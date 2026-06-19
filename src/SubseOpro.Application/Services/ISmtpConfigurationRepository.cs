namespace SubseOpro.Application.Services;

public record SmtpConfigurationDto(
    string Host,
    int Port,
    string Username,
    string Password,
    string FromEmail,
    string FromName,
    bool UseTls,
    bool IsActive);

public record SaveSmtpConfigurationInput(
    string Host,
    int Port,
    string? Username,
    string? Password,
    string FromEmail,
    string? FromName,
    bool UseTls,
    bool IsActive);

public record SendSmtpTestInput(string? ToEmail);

public record SendSmtpEmailInput(string ToEmail, string Subject, string Body, bool IsHtml);

public record SmtpSaveResult(bool Saved, DateTime UpdatedAt);

public record SmtpTestResult(bool Sent, string ToEmail);

public record SmtpSendResult(bool Sent, string ToEmail, string Subject);

public record SmtpConnectionCheckResult(bool Connected, string Host, int Port, bool UseTls);

public interface ISmtpConfigurationRepository
{
    Task<SmtpConfigurationDto> GetAsync(CancellationToken cancellationToken = default);
    Task<SmtpSaveResult> SaveAsync(SaveSmtpConfigurationInput input, CancellationToken cancellationToken = default);
    Task<SmtpConnectionCheckResult> ValidateConnectionAsync(CancellationToken cancellationToken = default);
    Task<SmtpTestResult> SendTestAsync(SendSmtpTestInput input, CancellationToken cancellationToken = default);
    Task<SmtpSendResult> SendAsync(SendSmtpEmailInput input, CancellationToken cancellationToken = default);
}
