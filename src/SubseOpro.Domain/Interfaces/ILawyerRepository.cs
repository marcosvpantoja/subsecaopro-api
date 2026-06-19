using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface ILawyerRepository : IRepository<Lawyer>
{
    Task<Lawyer?> GetByOabNumberAsync(string oabNumber, CancellationToken ct = default);
    Task<Lawyer?> GetByOabNumberGlobalAsync(string oabNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Lawyer>> SearchAsync(string? query, string? specialty, string? city, bool? online, CancellationToken ct = default);
    Task<bool> ValidateCredentialsAsync(string oabNumber, string password, CancellationToken ct = default);
    Task UpdateProfileAsync(Lawyer lawyer, IEnumerable<string> specialties, CancellationToken ct = default);
    Task UpdatePasswordHashAsync(Guid lawyerId, string passwordHash, CancellationToken ct = default);
}
