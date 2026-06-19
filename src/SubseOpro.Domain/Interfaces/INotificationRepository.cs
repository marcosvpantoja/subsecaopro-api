using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetVisibleAsync(bool isLawyer, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetUnreadForUserAsync(Guid userId, bool isLawyer, CancellationToken ct = default);
    Task<int> MarkAllAsReadAsync(Guid userId, bool isLawyer, CancellationToken ct = default);
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, bool isLawyer, CancellationToken ct = default);
}
