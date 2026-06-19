namespace SubseOpro.Application.Services;

public interface IPasswordHasher
{
    string TimingSafeDummyHash { get; }
    string Hash(string password);
    bool Verify(string password, string hash);
}
