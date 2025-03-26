using Hoot.Models;

namespace Hoot.Services.Users
{
    public interface IUserService
    {
        ValueTask<IEnumerable<UserViewModel>?> Get();
        ValueTask<UserViewModel?> GetById();
    }
}