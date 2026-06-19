using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface ISocialAccountRepository : IRepository<SocialAccount>
{
    Task<IReadOnlyList<SocialAccount>> GetOrderedAsync(CancellationToken ct = default);
}
