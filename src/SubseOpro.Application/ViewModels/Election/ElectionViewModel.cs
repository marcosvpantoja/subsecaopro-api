namespace SubseOpro.Application.ViewModels.Election;

public class ElectionViewModel
{
    public string Id { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Term { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public string DaysRemaining { get; set; } = string.Empty;
    public string PortalUrl { get; set; } = string.Empty;
    public int TotalSlates { get; set; }
    public List<CandidateViewModel> Candidates { get; set; } = [];
}
