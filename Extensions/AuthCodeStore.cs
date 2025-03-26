namespace Hoot.Extensions
{
    public static class AuthCodeStore
    {
        private static readonly Dictionary<string, string> _codes = new();

        public static void Store(string code, string clientId)
        {
            _codes[code] = clientId;
        }

        public static bool IsValid(string code)
        {
            return _codes.ContainsKey(code);
        }

        public static void Remove(string code)
        {
            _codes.Remove(code);
        }
    }

}
