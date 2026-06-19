using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.Services;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Post>> GetRecentAsync(CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync(ct);
    }

    public async Task<Post> LikePostAsync(Guid postId, CancellationToken ct = default)
    {
        var post = await DbSet.FirstOrDefaultAsync(p => p.Id == postId, ct)
            ?? throw new InvalidOperationException("Post não encontrado.");

        post.LikesCount++;
        await Context.SaveChangesAsync(ct);
        return post;
    }

    public async Task<Post?> GetByIdNoTrackingAsync(Guid postId, CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId, ct);
    }
}

public class SocialAccountRepository : Repository<SocialAccount>, ISocialAccountRepository
{
    public SocialAccountRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SocialAccount>> GetOrderedAsync(CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(s => s.Platform)
            .ThenBy(s => s.Handle)
            .ToListAsync(ct);
    }
}

public class AdminUserRepository : Repository<AdminUser>, IAdminUserRepository
{
    private readonly IPasswordHasher _passwordHasher;

    public AdminUserRepository(AppDbContext context, IPasswordHasher passwordHasher) : base(context)
    {
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email != null && a.Email.ToLower() == normalizedEmail, ct);
        if (user is null || !user.IsActive) return false;
        return _passwordHasher.Verify(password, user.PasswordHash);
    }

    public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email != null && a.Email.ToLower() == normalizedEmail, ct);
    }

    public async Task<AdminUser?> GetByEmailGlobalAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await Context.AdminUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email.ToLower() == normalizedEmail, ct);
    }

    public async Task<IReadOnlyList<AdminUser>> GetBySubsectionIdAsync(Guid subsectionId, CancellationToken ct = default)
    {
        return await Context.AdminUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.SubsectionId == subsectionId)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);
    }

    public async Task<AdminUser?> GetByIdForSubsectionAsync(Guid id, Guid subsectionId, CancellationToken ct = default)
    {
        return await Context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id && a.SubsectionId == subsectionId, ct);
    }

    public async Task<bool> EmailExistsGloballyAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var query = Context.AdminUsers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.Email.ToLower() == normalizedEmail);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<bool> UpdatePasswordHashByEmailAsync(string email, string passwordHash, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var admin = await Context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Email.ToLower() == normalizedEmail && a.IsActive, ct);

        if (admin is null)
            return false;

        admin.PasswordHash = passwordHash;
        Context.AdminUsers.Update(admin);
        await Context.SaveChangesAsync(ct);
        return true;
    }
}

public class BannerRepository : Repository<Banner>, IBannerRepository
{
    public BannerRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Banner>> GetOrderedAsync(CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderBy(b => b.SortOrder)
            .ToListAsync(ct);
    }
}

public class PartnerRepository : Repository<Partner>, IPartnerRepository
{
    public PartnerRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Partner>> GetByCategoryAsync(
        SubseOpro.Domain.Enums.PartnerCategory? category, CancellationToken ct = default)
    {
        var q = DbSet.AsNoTracking().AsQueryable();

        if (category.HasValue)
            q = q.Where(p => p.Category == category.Value);

        return await q.OrderBy(p => p.Name).ToListAsync(ct);
    }
}

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Notification>> GetVisibleAsync(bool isLawyer, CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(n => !n.ForLawyerOnly || isLawyer)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Notification>> GetUnreadForUserAsync(
        Guid userId,
        bool isLawyer,
        CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(n => (!n.ForLawyerOnly || isLawyer)
                && !Context.NotificationReads.Any(r => r.NotificationId == n.Id && r.UserId == userId))
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, bool isLawyer, CancellationToken ct = default)
    {
        var unreadIds = await DbSet
            .AsNoTracking()
            .Where(n => (!n.ForLawyerOnly || isLawyer)
                && !Context.NotificationReads.Any(r => r.NotificationId == n.Id && r.UserId == userId))
            .Select(n => n.Id)
            .ToListAsync(ct);

        if (unreadIds.Count == 0) return 0;

        var now = DateTime.UtcNow;
        var reads = unreadIds.Select(id => new NotificationRead
        {
            NotificationId = id,
            UserId = userId,
            ReadAt = now,
        });

        await Context.NotificationReads.AddRangeAsync(reads, ct);
        await Context.SaveChangesAsync(ct);
        return unreadIds.Count;
    }

    public async Task<bool> MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        bool isLawyer,
        CancellationToken ct = default)
    {
        var notification = await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId, ct);

        if (notification is null) return false;
        if (notification.ForLawyerOnly && !isLawyer) return false;

        var alreadyRead = await Context.NotificationReads
            .AnyAsync(r => r.NotificationId == notificationId && r.UserId == userId, ct);

        if (alreadyRead) return false;

        await Context.NotificationReads.AddAsync(new NotificationRead
        {
            NotificationId = notificationId,
            UserId = userId,
            ReadAt = DateTime.UtcNow,
        }, ct);
        await Context.SaveChangesAsync(ct);
        return true;
    }
}

public class ArticleRepository : Repository<Article>, IArticleRepository
{
    public ArticleRepository(AppDbContext context) : base(context) { }

    public async Task<Article?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Slug == slug, ct);
    }
}
