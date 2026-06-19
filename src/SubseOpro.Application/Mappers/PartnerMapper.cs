using System.Globalization;
using SubseOpro.Application.ViewModels.Partner;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;

namespace SubseOpro.Application.Mappers;

public static class PartnerMapper
{
    public static PartnerViewModel ToViewModel(Partner entity)
    {
        return new PartnerViewModel
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Category = ToCategoryLabel(entity.Category),
            Discount = entity.Discount,
            Description = entity.Description,
            Validity = entity.ValidUntil.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            Color = entity.Color,
            CouponCode = entity.CouponCode,
            CreatedAt = entity.CreatedAt.ToString("o", CultureInfo.InvariantCulture),
        };
    }

    public static string GenerateCouponCode(string name)
    {
        var slug = new string(name
            .Where(char.IsLetterOrDigit)
            .Take(10)
            .ToArray())
            .ToUpperInvariant();

        return string.IsNullOrWhiteSpace(slug) ? "CUPOM" : slug;
    }

    public static List<PartnerViewModel> ToViewModelList(IEnumerable<Partner> entities)
        => entities.Select(ToViewModel).ToList();

    public static PartnerCategory? ToCategoryEnum(string? category)
    {
        return category?.ToLowerInvariant() switch
        {
            "saúde" or "saude" => PartnerCategory.Saude,
            "educação" or "educacao" => PartnerCategory.Educacao,
            "alimentação" or "alimentacao" => PartnerCategory.Alimentacao,
            "tecnologia" => PartnerCategory.Tecnologia,
            "serviços" or "servicos" => PartnerCategory.Servicos,
            _ => null,
        };
    }

    private static string ToCategoryLabel(PartnerCategory category) => category switch
    {
        PartnerCategory.Saude => "Saúde",
        PartnerCategory.Educacao => "Educação",
        PartnerCategory.Alimentacao => "Alimentação",
        PartnerCategory.Tecnologia => "Tecnologia",
        PartnerCategory.Servicos => "Serviços",
        _ => category.ToString(),
    };
}
