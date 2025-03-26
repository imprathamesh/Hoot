using Hoot.Models;

namespace Hoot.Services
{
    public interface IClientService
    {
        ValueTask<bool> Delete(Guid id);
        ValueTask<IEnumerable<Client>?> Get();
        ValueTask<bool> Post(Client client);
    }
}