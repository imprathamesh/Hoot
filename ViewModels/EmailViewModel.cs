using System.ComponentModel.DataAnnotations;

namespace Hoot.ViewModels;

public class EmailViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

}
