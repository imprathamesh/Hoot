namespace Hoot.Helpers;

public static class ValidateAuthorizationRequest
{
    private static Dictionary<string, (string userId, DateTime expiresAt)> _authCodes = new();

    /// <summary>
    /// Check clienti valid with client_id, redirect_uri
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="redirectUri"></param>
    /// <returns></returns>
    public static bool IsValidClient(string clientId, string redirectUri)
    {
        // Example: A dictionary of allowed clients and their redirect URIs
        var allowedClients = new Dictionary<string, string>
{
    { "clientone", "https://localhost:7067/authentication/login-callback" }
};

        return allowedClients.TryGetValue(clientId, out var validRedirectUri) && validRedirectUri == redirectUri;
    }

    public static string GenerateAuthorizationCode(string userId, string clientId)
    {
        var code = Guid.NewGuid().ToString("N"); // Generate a random code
        _authCodes[code] = (userId, DateTime.UtcNow.AddMinutes(10)); // Store for 10 mins
        return code;
    }
    public static string? ValidateAuthorizationCode(string code)
    {
        if (_authCodes.TryGetValue(code, out var data))
        {
            if (data.expiresAt > DateTime.UtcNow)
            {
                _authCodes.Remove(code); // One-time use code, remove after validation
                return data.userId;
            }
        }
        return null;
    }
}
