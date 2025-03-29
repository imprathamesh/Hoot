using Hoot.ViewModels;

namespace Hoot.Services.Users
{
    public interface IUserService
    {
        ValueTask<IEnumerable<UserViewModel>?> Get();
        ValueTask<UserViewModel?> GetById();
        Dictionary<string, string> GetClaims();
        Task<bool> GetUser();
    }
}