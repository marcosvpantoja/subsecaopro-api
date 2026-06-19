using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface IBannerRepository : IRepository<Banner>
{
    Task<IReadOnlyList<Banner>> GetOrderedAsync(CancellationToken ct = default);
}
