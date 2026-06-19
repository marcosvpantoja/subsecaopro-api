using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface IPlatformAdminRepository
{
    Task<PlatformAdmin?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<PlatformAdmin?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
}
