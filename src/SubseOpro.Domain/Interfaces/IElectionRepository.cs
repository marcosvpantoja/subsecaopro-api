using SubseOpro.Domain.Entities;

namespace SubseOpro.Domain.Interfaces;

public interface IElectionRepository
{
    Task<Election?> GetCurrentAsync(CancellationToken ct = default);
}
