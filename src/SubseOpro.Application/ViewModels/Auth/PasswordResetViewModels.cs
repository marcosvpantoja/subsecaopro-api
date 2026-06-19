namespace SubseOpro.Application.ViewModels.Auth;

public class ForgotPasswordRequestViewModel
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
