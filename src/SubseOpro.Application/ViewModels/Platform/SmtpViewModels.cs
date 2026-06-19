namespace SubseOpro.Application.ViewModels.Platform;

public class SmtpConfigViewModel
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool UseTls { get; set; } = true;
    public bool IsActive { get; set; }
}

public class SaveSmtpConfigRequestViewModel
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public bool UseTls { get; set; } = true;
    public bool IsActive { get; set; }
}

public class SendSmtpTestRequestViewModel
{
    public string? ToEmail { get; set; }
}

public class SmtpSaveViewModel
{
    public bool Saved { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SmtpTestViewModel
{
    public bool Sent { get; set; }
    public string ToEmail { get; set; } = string.Empty;
}

public class SmtpConnectionViewModel
{
    public bool Connected { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseTls { get; set; }
}
