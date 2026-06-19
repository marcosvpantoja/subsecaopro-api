using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface ISubsectionRepository
{
    Task<Subsection?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Subsection?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Subsection>> GetAllAsync(CancellationToken ct = default);
    Task<Subsection?> GetCurrentAsync(CancellationToken ct = default);
    Task AddAsync(Subsection subsection, CancellationToken ct = default);
    Task UpdateAsync(Subsection subsection, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
