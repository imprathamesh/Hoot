using Hoot.Data;
using Hoot.Models;
using Microsoft.EntityFrameworkCore;

namespace Hoot.Services;

public class ClientService : IClientService
{
    readonly ApplicationDbContext _context;

    public ClientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<IEnumerable<Client>?> Get()
    {
        return await _context.Client.ToListAsync();
    }
    public async ValueTask<Client?> GetById()
    {
        return await _context.Client.FirstOrDefaultAsync();
    }
    public async ValueTask<bool> Post(Client client)
    {
        _context.Client.Add(client);

        await _context.SaveChangesAsync();
        return true;
    }
    public async ValueTask<bool> Delete(Guid id)
    {
        var client = await _context.Client.FindAsync(id);

        if (client is null)
        {
            return false;
        }
        _context.Client.Remove(client);
        await _context.SaveChangesAsync();

        return true;
    }
}
