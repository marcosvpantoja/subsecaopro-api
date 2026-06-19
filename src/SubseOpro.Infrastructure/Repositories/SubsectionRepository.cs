using Microsoft.EntityFrameworkCore;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Repositories;

public class SubsectionRepository : ISubsectionRepository
{
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public SubsectionRepository(AppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Subsection?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _context.Subsections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Slug == slug, ct);
    }

    public async Task<Subsection?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Subsections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<Subsection>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Subsections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<Subsection?> GetCurrentAsync(CancellationToken ct = default)
    {
        if (!_tenantContext.SubsectionId.HasValue)
            return null;

        return await _context.Subsections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == _tenantContext.SubsectionId.Value, ct);
    }

    public async Task AddAsync(Subsection subsection, CancellationToken ct = default)
    {
        await _context.Subsections.AddAsync(subsection, ct);
    }

    public Task UpdateAsync(Subsection subsection, CancellationToken ct = default)
    {
        _context.Subsections.Update(subsection);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
