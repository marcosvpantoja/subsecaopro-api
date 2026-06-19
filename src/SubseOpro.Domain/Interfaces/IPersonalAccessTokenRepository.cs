using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface IPersonalAccessTokenRepository
{
    Task<PersonalAccessToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task<IReadOnlyList<PersonalAccessToken>> GetBySubsectionIdAsync(Guid subsectionId, CancellationToken ct = default);
    Task<PersonalAccessToken?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PersonalAccessToken> AddAsync(PersonalAccessToken entity, CancellationToken ct = default);
    Task RevokeAsync(PersonalAccessToken entity, CancellationToken ct = default);
    Task UpdateLastUsedAtAsync(Guid id, DateTimeOffset usedAt, CancellationToken ct = default);
}
