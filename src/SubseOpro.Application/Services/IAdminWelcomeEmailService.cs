using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Services;

public interface IAdminWelcomeEmailService
{
    Task<bool> TrySendWelcomeAsync(
        AdminUser admin,
        Subsection subsection,
        string plainPassword,
        CancellationToken cancellationToken = default);
}
