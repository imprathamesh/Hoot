using System.ComponentModel.DataAnnotations;

namespace Hoot.ViewModels;

public class UserViewModel
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedOn { get; internal set; }
}