using SubseOpro.Application.ViewModels.Auth;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class AdminMapper
{
    public static AuthUserViewModel ToAuthUserViewModel(AdminUser entity)
    {
        return new AuthUserViewModel
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Email = entity.Email,
            Active = entity.IsActive,
            Initials = GetInitials(entity.Name),
            AccountType = "backoffice",
            Roles = [entity.Role],
        };
    }

    private static string GetInitials(string name)
    {
        var parts = name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Take(2)
            .Select(p => p[0].ToString().ToUpperInvariant());

        return string.Concat(parts);
    }
}
