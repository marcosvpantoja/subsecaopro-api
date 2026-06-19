namespace SubseOpro.Domain.Entities;

public class Specialty
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public string Name { get; set; } = string.Empty;

    public ICollection<LawyerSpecialty> LawyerSpecialties { get; set; } = [];
}
