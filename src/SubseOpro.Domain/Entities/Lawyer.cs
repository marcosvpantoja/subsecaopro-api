namespace SubseOpro.Domain.Entities;

public class Lawyer
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string OabNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public bool Online { get; set; }
    public string Whatsapp { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LawyerSpecialty> LawyerSpecialties { get; set; } = [];
    public ICollection<Reservation> Reservations { get; set; } = [];
}
