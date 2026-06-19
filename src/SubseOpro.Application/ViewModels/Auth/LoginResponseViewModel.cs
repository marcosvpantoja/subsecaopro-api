namespace SubseOpro.Application.ViewModels.Auth;

public class LoginResponseViewModel
{
    public string Token { get; set; } = string.Empty;
    public AuthUserViewModel User { get; set; } = new();
}
