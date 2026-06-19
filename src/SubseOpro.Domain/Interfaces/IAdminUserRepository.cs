using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface IAdminUserRepository : IRepository<AdminUser>
{
    Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default);
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AdminUser?> GetByEmailGlobalAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<AdminUser>> GetBySubsectionIdAsync(Guid subsectionId, CancellationToken ct = default);
    Task<AdminUser?> GetByIdForSubsectionAsync(Guid id, Guid subsectionId, CancellationToken ct = default);
    Task<bool> EmailExistsGloballyAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> UpdatePasswordHashByEmailAsync(string email, string passwordHash, CancellationToken ct = default);
}
