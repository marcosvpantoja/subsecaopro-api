namespace SubseOpro.Domain.Entities;

public class Election
{
    public Guid Id { get; set; }
    public Guid SubsectionId { get; set; }
    public Subsection Subsection { get; set; } = null!;
    public int Year { get; set; }
    public string Term { get; set; } = string.Empty;
    public DateOnly EndDate { get; set; }
    public string PortalUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public List<Candidate> Candidates { get; set; } = [];
}
