namespace SubseOpro.Domain.Entities;

/// <summary>
/// Configuração SMTP global da plataforma (singleton — uma linha ativa por vez).
/// </summary>
public class SmtpConfiguration
{
    public Guid Id { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool UseTls { get; set; } = true;
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
