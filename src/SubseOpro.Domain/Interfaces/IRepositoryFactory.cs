namespace SubseOpro.Domain.Interfaces;

public interface IRepositoryFactory
{
    IAdminUserRepository AdminUsers { get; }
    ILawyerRepository Lawyers { get; }
    IRoomRepository Rooms { get; }
    IReservationRepository Reservations { get; }
    IPostRepository Posts { get; }
    ISocialAccountRepository SocialAccounts { get; }
    IBannerRepository Banners { get; }
    IPartnerRepository Partners { get; }
    INotificationRepository Notifications { get; }
    IArticleRepository Articles { get; }
    IElectionRepository Elections { get; }
    ISubsectionRepository Subsections { get; }
    IPlatformAdminRepository PlatformAdmins { get; }
    IPersonalAccessTokenRepository PersonalAccessTokens { get; }
}
