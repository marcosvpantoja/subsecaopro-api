using System.Security.Cryptography;
using System.Text;

namespace SubseOpro.Api.Services;

internal static class TokenHasher
{
    public static string ComputeSha256Hex(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
