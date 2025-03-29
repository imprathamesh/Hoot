namespace Hoot.Helpers.Store;
/// <summary>
/// Authorization Code Store. The authorization code generated to store and call the user id by authorization code
/// </summary>
public static class AuthCodeStore
{
    private static readonly Dictionary<string, string> _codes = new();

    /// <summary>
    /// Store the authorization code
    /// </summary>
    /// <param name="code"></param>
    /// <param name="clientId"></param>
    public static void Store(string code, string clientId)
    {
        _codes[code] = clientId;
    }

    /// <summary>
    /// Check the authorization code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static bool IsValid(string code)
    {
        return _codes.ContainsKey(code);
    }

    /// <summary>
    /// Get active user id
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static string GetUserId(string code)
    {
        return _codes.TryGetValue(code, out var userId) ? userId : null;
    }

    /// <summary>
    /// Remove the authorization code
    /// </summary>
    /// <param name="code"></param>
    public static void Remove(string code)
    {
        _codes.Remove(code);
    }
}
