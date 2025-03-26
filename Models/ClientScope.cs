using System.ComponentModel.DataAnnotations;

namespace Hoot.Models;

public class ClientScope
{
    public int Id { get; set; }
    [Required]
    public string Scope { get; set; }

    public int ClientId { get; set; }
    public virtual Client Client { get; set; }
}
