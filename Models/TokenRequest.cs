using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hoot.Models
{
    public class TokenRequest
    {
        [Required]
        [FromForm(Name = "client_id")]
        public string ClientId { get; set; }

        [Required]
        [FromForm(Name = "grant_type")]
        public string GrantType { get; set; } = "authorization_code"; // Should be "authorization_code"

        [Required]
        [FromForm(Name = "code")]
        public string Code { get; set; }

        [Required]
        [FromForm(Name = "redirect_uri")]
        public string RedirectUri { get; set; }

        [FromForm(Name = "code_verifier")]
        public string CodeVerifier { get; set; }

        //[FromForm(Name = "code_challenge")]
        //public string CodeChallenge { get; set; }
    }

    public class UserViewModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedOn { get; internal set; }
    }
}
