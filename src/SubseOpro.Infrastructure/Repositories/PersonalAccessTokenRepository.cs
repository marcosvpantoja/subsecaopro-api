using Microsoft.EntityFrameworkCore;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Repositories;

public class PersonalAccessTokenRepository : IPersonalAccessTokenRepository
{
    private readonly AppDbContext _context;

    public PersonalAccessTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PersonalAccessToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        return await _context.PersonalAccessTokens
            .Include(p => p.Subsection)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TokenHash == tokenHash, ct);
    }

    public async Task<IReadOnlyList<PersonalAccessToken>> GetBySubsectionIdAsync(Guid subsectionId, CancellationToken ct = default)
    {
        return await _context.PersonalAccessTokens
            .AsNoTracking()
            .Where(p => p.SubsectionId == subsectionId && !p.IsRevoked)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PersonalAccessToken?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PersonalAccessTokens
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PersonalAccessToken> AddAsync(PersonalAccessToken entity, CancellationToken ct = default)
    {
        await _context.PersonalAccessTokens.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task RevokeAsync(PersonalAccessToken entity, CancellationToken ct = default)
    {
        entity.IsRevoked = true;
        _context.PersonalAccessTokens.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateLastUsedAtAsync(Guid id, DateTimeOffset usedAt, CancellationToken ct = default)
    {
        await _context.PersonalAccessTokens
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.LastUsedAt, usedAt), ct);
    }
}
