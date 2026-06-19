namespace SubseOpro.Domain.Entities;

public class Candidate
{
    public Guid Id { get; set; }
    public Guid ElectionId { get; set; }
    public Election Election { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Slate { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ColorStart { get; set; } = string.Empty;
    public string ColorEnd { get; set; } = string.Empty;
}
