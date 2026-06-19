using System.Security.Cryptography;
using System.Text;

namespace SubseOpro.Api.Services;

public class AiTokenEncryptionService
{
    private readonly byte[] _key;

    public AiTokenEncryptionService(IConfiguration configuration)
    {
        var keyHex = configuration["Encryption:AiTokenKey"]
            ?? throw new InvalidOperationException("Encryption:AiTokenKey não configurado.");

        _key = Convert.FromHexString(keyHex);

        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption:AiTokenKey deve ter exatamente 32 bytes (64 caracteres hex).");
    }

    // Formato: "<iv_base64>.<ciphertext_base64>.<tag_base64>"
    public string Encrypt(string plaintext)
    {
        var iv = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];

        using var aes = new AesGcm(_key, 16);
        aes.Encrypt(iv, plaintextBytes, ciphertext, tag);

        return $"{Convert.ToBase64String(iv)}.{Convert.ToBase64String(ciphertext)}.{Convert.ToBase64String(tag)}";
    }

    public string Decrypt(string encrypted)
    {
        var parts = encrypted.Split('.');
        if (parts.Length != 3)
            throw new FormatException("Formato de token criptografado inválido.");

        var iv = Convert.FromBase64String(parts[0]);
        var ciphertext = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, 16);
        aes.Decrypt(iv, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}
