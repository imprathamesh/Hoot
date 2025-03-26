using System.ComponentModel.DataAnnotations;

namespace Hoot.Models;

public class Client
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ClientName { get; set; }
    public string ClientUri { get; set; }
    public string Description { get; set; }

    [Required]
    public string ClientId { get; set; }

    [Required]
    public string ClientSecret { get; set; } // Optional if using PKCE

    [Required]
    public string RedirectUri { get; set; }

    [Required]
    public string Scopes { get; set; } // Store as comma-separated values (e.g., "openid,profile,email")

    public bool RequirePkce { get; set; } = true;
    public bool Enabled { get; set; } = true;

    public virtual ICollection<ClientPostLogoutRedirectUri> ClientPostLogoutRedirectUris { get; set; }
    public virtual ICollection<ClientCorsOrigin> ClientCorsOrigins { get; set; }
    public virtual ICollection<ClientRedirectUri> ClientRedirectUris { get; set; }
    public virtual ICollection<ClientScope> ClientScopes { get; set; }
    public virtual ICollection<ClientSecret> ClientSecrets { get; set; }

    public Client()
    {
        ClientPostLogoutRedirectUris = new HashSet<ClientPostLogoutRedirectUri>();
        ClientCorsOrigins = new HashSet<ClientCorsOrigin>();
        ClientRedirectUris = new HashSet<ClientRedirectUri>();
        ClientScopes = new HashSet<ClientScope>();
        ClientSecrets = new HashSet<ClientSecret>();
    }
}