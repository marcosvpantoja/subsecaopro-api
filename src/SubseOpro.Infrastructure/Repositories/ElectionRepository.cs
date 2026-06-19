using Microsoft.EntityFrameworkCore;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Repositories;

public class ElectionRepository : IElectionRepository
{
    private readonly AppDbContext _context;

    public ElectionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Election?> GetCurrentAsync(CancellationToken ct = default)
    {
        return await _context.Elections
            .Include(e => e.Candidates.OrderBy(c => c.Number))
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IsActive, ct);
    }
}
