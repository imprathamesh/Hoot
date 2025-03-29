using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hoot.ViewModels;

public class TokenRequestViewModel
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

    //[FromForm(Name = "code_verifier")]
    //public string CodeVerifier { get; set; }

    //[FromForm(Name = "code_challenge")]
    //public string CodeChallenge { get; set; }
}


