using SubseOpro.Application.ViewModels.Notification;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class NotificationMapper
{
    public static NotificationViewModel ToViewModel(Notification entity)
    {
        return new NotificationViewModel
        {
            Id = entity.Id.ToString(),
            Icon = entity.Icon,
            Text = entity.Text,
            Date = FormatRelativeDate(entity.CreatedAt),
            ForLawyer = entity.ForLawyerOnly ? true : null,
            Href = entity.Href,
        };
    }

    public static List<NotificationViewModel> ToViewModelList(IEnumerable<Notification> entities)
        => entities.Select(ToViewModel).ToList();

    private static string FormatRelativeDate(DateTime createdAt)
    {
        var diff = DateTime.UtcNow - createdAt;
        if (diff.TotalHours <= 1) return "há 1h";
        if (diff.TotalHours <= 4) return "há 3h";
        if (diff.TotalDays <= 2) return "ontem";
        return createdAt.ToString("dd/MM/yyyy");
    }
}
