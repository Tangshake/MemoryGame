
using LoginService.Entity;

namespace LoginService.DatabaseAccess
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(int id);
    }
}
