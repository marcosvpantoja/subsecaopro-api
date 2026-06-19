using Microsoft.EntityFrameworkCore;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Subsection> Subsections => Set<Subsection>();
    public DbSet<PlatformAdmin> PlatformAdmins => Set<PlatformAdmin>();
    public DbSet<PersonalAccessToken> PersonalAccessTokens => Set<PersonalAccessToken>();
    public DbSet<PlatformHubSettings> PlatformHubSettings => Set<PlatformHubSettings>();
    public DbSet<PlatformAppSettings> PlatformAppSettings => Set<PlatformAppSettings>();
    public DbSet<SmtpConfiguration> SmtpConfigurations => Set<SmtpConfiguration>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();
    public DbSet<Lawyer> Lawyers => Set<Lawyer>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<LawyerSpecialty> LawyerSpecialties => Set<LawyerSpecialty>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<SocialAccount> SocialAccounts => Set<SocialAccount>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationRead> NotificationReads => Set<NotificationRead>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Election> Elections => Set<Election>();
    public DbSet<Candidate> Candidates => Set<Candidate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ApplyTenantQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lawyer>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<AdminUser>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<Room>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<Reservation>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<Post>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<SocialAccount>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<Banner>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<Partner>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<Notification>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<Article>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<Election>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);

        modelBuilder.Entity<Specialty>().HasQueryFilter(e =>
            _tenantContext.SubsectionId == null || e.SubsectionId == _tenantContext.SubsectionId);
    }
}
