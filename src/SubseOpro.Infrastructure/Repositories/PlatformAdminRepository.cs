using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.Services;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Repositories;

public class PlatformAdminRepository : IPlatformAdminRepository
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public PlatformAdminRepository(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<PlatformAdmin?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.PlatformAdmins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email == email, ct);
    }

    public async Task<PlatformAdmin?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PlatformAdmins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            return false;

        var admin = await _context.PlatformAdmins
            .FirstOrDefaultAsync(a => a.Email == email, ct);

        if (admin is null || !admin.IsActive || string.IsNullOrEmpty(admin.PasswordHash))
        {
            _passwordHasher.Verify(password, _passwordHasher.TimingSafeDummyHash);
            return false;
        }

        return _passwordHasher.Verify(password, admin.PasswordHash);
    }
}
