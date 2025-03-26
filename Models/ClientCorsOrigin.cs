using System.ComponentModel.DataAnnotations;

namespace Hoot.Models;

public class ClientCorsOrigin
{
    public int Id { get; set; }
    [Required]
    public string Url { get; set; }
    public int ClientId { get; set; }
    public virtual Client Client { get; set; }
}
