namespace SubseOpro.Application.Services;

public static class PasswordPolicy
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static bool IsValid(string password, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(password))
        {
            error = "A senha é obrigatória.";
            return false;
        }

        if (password.Length < MinLength)
        {
            error = $"A senha deve ter no mínimo {MinLength} caracteres.";
            return false;
        }

        if (password.Length > MaxLength)
        {
            error = $"A senha deve ter no máximo {MaxLength} caracteres.";
            return false;
        }

        if (!password.Any(char.IsLetter))
        {
            error = "A senha deve conter pelo menos uma letra.";
            return false;
        }

        if (!password.Any(char.IsDigit))
        {
            error = "A senha deve conter pelo menos um número.";
            return false;
        }

        return true;
    }
}
