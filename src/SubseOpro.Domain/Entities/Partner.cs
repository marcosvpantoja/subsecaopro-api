using SubseOpro.Domain.Enums;

namespace SubseOpro.Domain.Entities;

public class Partner
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public PartnerCategory Category { get; set; }
    public string Discount { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly ValidUntil { get; set; }
    public string Color { get; set; } = string.Empty;
    public string CouponCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
