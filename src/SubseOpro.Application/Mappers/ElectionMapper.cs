using System.Globalization;
using SubseOpro.Application.ViewModels.Election;
using SubseOpro.Domain.Entities;

namespace SubseOpro.Application.Mappers;

public static class ElectionMapper
{
    public static ElectionViewModel ToViewModel(Election entity)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var daysLeft = entity.EndDate.DayNumber - today.DayNumber;
        var daysRemaining = daysLeft switch
        {
            <= 0 => "encerrada",
            1 => "encerra amanhã",
            _ => $"encerra em {daysLeft} dias",
        };

        return new ElectionViewModel
        {
            Id = entity.Id.ToString(),
            Year = entity.Year,
            Term = entity.Term,
            EndDate = entity.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            DaysRemaining = daysRemaining,
            PortalUrl = entity.PortalUrl,
            TotalSlates = entity.Candidates.Count,
            Candidates = entity.Candidates.Select(CandidateToViewModel).ToList(),
        };
    }

    private static CandidateViewModel CandidateToViewModel(Candidate c) =>
        new()
        {
            Id = c.Id.ToString(),
            Name = c.Name,
            Slate = c.Slate,
            Number = c.Number,
            Bio = c.Bio,
            Colors = [c.ColorStart, c.ColorEnd],
        };
}
