using Microsoft.Extensions.Configuration;
using SubseOpro.Application.Services;

namespace SubseOpro.Infrastructure.Security;

public class BcryptPasswordHasher : IPasswordHasher
{
    private readonly int _workFactor;

    public string TimingSafeDummyHash { get; }

    public BcryptPasswordHasher(IConfiguration configuration)
    {
        _workFactor = configuration.GetValue("Auth:BcryptWorkFactor", 12);
        if (_workFactor is < 10 or > 14)
            throw new InvalidOperationException("Auth:BcryptWorkFactor deve estar entre 10 e 14.");

        TimingSafeDummyHash = BCrypt.Net.BCrypt.HashPassword("__timing_safe_dummy__", _workFactor);
    }

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, _workFactor);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
