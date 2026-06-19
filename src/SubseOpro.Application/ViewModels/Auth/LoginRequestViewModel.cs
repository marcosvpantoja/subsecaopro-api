namespace SubseOpro.Application.ViewModels.Auth;

public class LoginRequestViewModel
{
    public string? Oab { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
}
