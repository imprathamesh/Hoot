namespace Hoot.Models
{
    public class ClientPostLogoutRedirectUri
    {
        public int Id { get; set; }
        public string Uri { get; set; }

        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
    }
}
