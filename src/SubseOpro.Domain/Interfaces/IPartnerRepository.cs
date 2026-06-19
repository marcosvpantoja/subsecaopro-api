using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;

namespace SubseOpro.Domain.Interfaces;

public interface IPartnerRepository : IRepository<Partner>
{
    Task<IReadOnlyList<Partner>> GetByCategoryAsync(PartnerCategory? category, CancellationToken ct = default);
}
