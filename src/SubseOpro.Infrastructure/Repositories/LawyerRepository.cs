using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.Services;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Repositories;

public class LawyerRepository : Repository<Lawyer>, ILawyerRepository
{
    private readonly IPasswordHasher _passwordHasher;

    public LawyerRepository(AppDbContext context, IPasswordHasher passwordHasher) : base(context)
    {
        _passwordHasher = passwordHasher;
    }

    public async Task<Lawyer?> GetByOabNumberGlobalAsync(string oabNumber, CancellationToken ct = default)
    {
        var normalized = NormalizeOab(oabNumber);
        var digits = new string(oabNumber.Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return null;

        var lawyers = await Context.Lawyers
            .IgnoreQueryFilters()
            .Include(l => l.LawyerSpecialties)
                .ThenInclude(ls => ls.Specialty)
            .ToListAsync(ct);

        return lawyers.FirstOrDefault(l =>
            NormalizeOab(l.OabNumber) == normalized ||
            new string(l.OabNumber.Where(char.IsDigit).ToArray()).EndsWith(digits));
    }

    public async Task<Lawyer?> GetByOabNumberAsync(string oabNumber, CancellationToken ct = default)
    {
        var normalized = NormalizeOab(oabNumber);
        var digits = new string(oabNumber.Where(char.IsDigit).ToArray());
        if (digits.Length == 0) return null;

        var lawyers = await DbSet
            .Include(l => l.LawyerSpecialties)
                .ThenInclude(ls => ls.Specialty)
            .ToListAsync(ct);

        return lawyers.FirstOrDefault(l =>
            NormalizeOab(l.OabNumber) == normalized ||
            new string(l.OabNumber.Where(char.IsDigit).ToArray()).EndsWith(digits));
    }

    public override async Task<IReadOnlyList<Lawyer>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet
            .Include(l => l.LawyerSpecialties)
                .ThenInclude(ls => ls.Specialty)
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync(ct);
    }

    public override async Task<Lawyer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(l => l.LawyerSpecialties)
                .ThenInclude(ls => ls.Specialty)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<IReadOnlyList<Lawyer>> SearchAsync(
        string? query, string? specialty, string? city, bool? online, CancellationToken ct = default)
    {
        var q = DbSet
            .Include(l => l.LawyerSpecialties)
                .ThenInclude(ls => ls.Specialty)
            .AsNoTracking()
            .Where(l => l.IsPublic && l.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(l =>
                l.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                l.Bio.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                l.City.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                l.LawyerSpecialties.Any(ls => ls.Specialty.Name.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(specialty))
            q = q.Where(l => l.LawyerSpecialties.Any(ls => ls.Specialty.Name == specialty));

        if (!string.IsNullOrWhiteSpace(city))
            q = q.Where(l => l.City == city);

        if (online.HasValue)
            q = q.Where(l => l.Online == online.Value);

        return await q.OrderByDescending(l => l.Rating).ToListAsync(ct);
    }

    public async Task<bool> ValidateCredentialsAsync(string oabNumber, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(oabNumber) || string.IsNullOrEmpty(password))
            return false;

        var lawyer = await GetByOabNumberAsync(oabNumber, ct);

        if (lawyer is null || !lawyer.IsActive || string.IsNullOrEmpty(lawyer.PasswordHash))
        {
            _passwordHasher.Verify(password, _passwordHasher.TimingSafeDummyHash);
            return false;
        }

        return _passwordHasher.Verify(password, lawyer.PasswordHash);
    }

    public async Task UpdatePasswordHashAsync(Guid lawyerId, string passwordHash, CancellationToken ct = default)
    {
        var lawyer = await DbSet.FirstOrDefaultAsync(l => l.Id == lawyerId, ct)
            ?? throw new InvalidOperationException("Advogado não encontrado.");

        lawyer.PasswordHash = passwordHash;
        await Context.SaveChangesAsync(ct);
    }

    public async Task UpdateProfileAsync(Lawyer lawyer, IEnumerable<string> specialties, CancellationToken ct = default)
    {
        var existing = await DbSet
            .Include(l => l.LawyerSpecialties)
            .FirstOrDefaultAsync(l => l.Id == lawyer.Id, ct);

        if (existing is null) throw new InvalidOperationException("Advogado não encontrado.");

        existing.Name = lawyer.Name;
        existing.City = lawyer.City;
        existing.Bio = lawyer.Bio;
        existing.Whatsapp = lawyer.Whatsapp;
        existing.Email = lawyer.Email;
        existing.Address = lawyer.Address;
        existing.Availability = lawyer.Availability;
        existing.Online = lawyer.Online;
        existing.PhotoUrl = lawyer.PhotoUrl;

        Context.LawyerSpecialties.RemoveRange(existing.LawyerSpecialties);

        foreach (var specName in specialties)
        {
            var specialty = await Context.Specialties
                .FirstOrDefaultAsync(s => s.Name == specName, ct);

            if (specialty is null)
            {
                specialty = new Specialty { Id = Guid.NewGuid(), SubsectionId = lawyer.SubsectionId, Name = specName };
                await Context.Specialties.AddAsync(specialty, ct);
            }

            existing.LawyerSpecialties.Add(new LawyerSpecialty
            {
                LawyerId = existing.Id,
                SpecialtyId = specialty.Id,
            });
        }

        await Context.SaveChangesAsync(ct);
    }

    private static string NormalizeOab(string oab)
        => oab.Replace(".", "").Replace("/", "").Replace(" ", "").Trim();
}
