namespace SubseOpro.Application.ViewModels.Election;

public class CandidateViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slate { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string[] Colors { get; set; } = [];
}
