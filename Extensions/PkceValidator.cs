using System.Security.Cryptography;
using System.Text;
namespace Hoot.Extensions;

public static class PkceValidator
{
    public static bool Validate(string codeVerifier, string codeChallenge)
    {
        if (string.IsNullOrEmpty(codeVerifier) || string.IsNullOrEmpty(codeChallenge))
            return false;

        using var sha256 = SHA256.Create();
        var computedChallenge = Convert.ToBase64String(
            sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier)))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return computedChallenge == codeChallenge;
    }
}
