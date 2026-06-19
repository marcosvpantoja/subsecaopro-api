namespace SubseOpro.Domain.Entities;

public class LawyerSpecialty
{
    public Guid LawyerId { get; set; }
    public Guid SpecialtyId { get; set; }

    public Lawyer Lawyer { get; set; } = null!;
    public Specialty Specialty { get; set; } = null!;
}
