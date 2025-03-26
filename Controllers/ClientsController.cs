using Hoot.Data;
using Hoot.Helpers;
using Hoot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hoot.Controllers;

[Authorize(Roles = Roles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async ValueTask<ActionResult<IEnumerable<Client>>> Get()
    {
        return await _context.Client.ToListAsync();
    }
}
