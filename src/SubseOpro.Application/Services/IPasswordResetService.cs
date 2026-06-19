namespace SubseOpro.Application.Services;

public interface IPasswordResetService
{
    Task RequestResetAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);
}
