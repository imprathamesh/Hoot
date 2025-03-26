using System.ComponentModel.DataAnnotations;

namespace Hoot.Models;

public class ClientSecret
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string? Description { get; set; }
    [Required]
    public string Value { get; set; }
    public DateTime ExpireOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public virtual Client Client { get; set; }
}
