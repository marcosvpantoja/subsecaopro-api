using SubseOpro.Application.ViewModels.Auth;
using SubseOpro.Application.ViewModels.Lawyer;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class LawyerMapper
{
    public static LawyerViewModel ToViewModel(Lawyer entity)
    {
        return new LawyerViewModel
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Oab = entity.OabNumber,
            City = entity.City,
            Specialties = entity.LawyerSpecialties
                .Select(ls => ls.Specialty.Name)
                .ToList(),
            Bio = entity.Bio,
            Rating = entity.Rating,
            Online = entity.Online,
            Whatsapp = entity.Whatsapp,
            Email = entity.Email,
            Address = entity.Address,
            Availability = entity.Availability,
            Color = entity.Color,
            Initials = entity.Initials,
            Photo = entity.PhotoUrl,
            IsActive = entity.IsActive,
        };
    }

    public static List<LawyerViewModel> ToViewModelList(IEnumerable<Lawyer> entities)
        => entities.Select(ToViewModel).ToList();

    public static AuthUserViewModel ToAuthUserViewModel(Lawyer entity)
    {
        var specialty = entity.LawyerSpecialties.FirstOrDefault()?.Specialty.Name ?? string.Empty;
        return new AuthUserViewModel
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Oab = entity.OabNumber,
            Specialty = specialty,
            City = entity.City,
            Email = entity.Email,
            Whatsapp = entity.Whatsapp,
            Bio = entity.Bio,
            Active = entity.IsActive,
            Initials = entity.Initials,
            AccountType = "lawyer",
            Roles = ["lawyer"],
        };
    }

    public static void UpdateEntity(Lawyer entity, LawyerUpdateViewModel vm)
    {
        entity.Name = vm.Name;
        entity.City = vm.City;
        entity.Bio = vm.Bio;
        entity.Whatsapp = vm.Whatsapp;
        entity.Email = vm.Email;
        entity.Address = vm.Address;
        entity.Availability = vm.Availability;
        entity.Online = vm.Online;
        entity.PhotoUrl = vm.Photo;
    }
}
