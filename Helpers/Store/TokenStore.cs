namespace Hoot.Helpers.Store
{
    public class TokenStore
    {
        private static readonly Dictionary<string, string> _codes = new();
        readonly IHttpContextAccessor _contextAcc;

        public TokenStore(IHttpContextAccessor contextAcc)
        {
            _contextAcc = contextAcc;
        }

        public static void Store(string code, string clientId)
        {
            _codes[code] = clientId;
        }

        public static bool IsValid(string code)
        {
            return _codes.ContainsKey(code);
        }
        public static string GetTokenToCookie(string code)
        {
            if (_codes.TryGetValue(code, out var userId))
            {
                return userId;
            }

            return null;
        }
        public static void Remove(string code)
        {
            _codes.Remove(code);
        }
    }
}
