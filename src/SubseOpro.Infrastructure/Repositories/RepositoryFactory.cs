using SubseOpro.Application.Services;
using SubseOpro.Domain.Interfaces;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Repositories;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;

    public RepositoryFactory(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        ITenantContext tenantContext)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
    }

    public IAdminUserRepository AdminUsers => new AdminUserRepository(_context, _passwordHasher);
    public ILawyerRepository Lawyers => new LawyerRepository(_context, _passwordHasher);
    public IRoomRepository Rooms => new RoomRepository(_context);
    public IReservationRepository Reservations => new ReservationRepository(_context);
    public IPostRepository Posts => new PostRepository(_context);
    public ISocialAccountRepository SocialAccounts => new SocialAccountRepository(_context);
    public IBannerRepository Banners => new BannerRepository(_context);
    public IPartnerRepository Partners => new PartnerRepository(_context);
    public INotificationRepository Notifications => new NotificationRepository(_context);
    public IArticleRepository Articles => new ArticleRepository(_context);
    public IElectionRepository Elections => new ElectionRepository(_context);
    public ISubsectionRepository Subsections => new SubsectionRepository(_context, _tenantContext);
    public IPlatformAdminRepository PlatformAdmins => new PlatformAdminRepository(_context, _passwordHasher);
    public IPersonalAccessTokenRepository PersonalAccessTokens => new PersonalAccessTokenRepository(_context);
}
