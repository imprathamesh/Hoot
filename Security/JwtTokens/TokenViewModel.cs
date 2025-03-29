namespace Hoot.Security.JwtTokens;

public class TokenViewModel
{
    public string Id_token { get; set; }
    public string Access_token { get; set; }
    public string Refresh_token { get; set; }
    public string Token_type { get => "Bearer"; }
    public int Expires_in { get; set; } = 3600;
}
