using RegisterService.Entity;

namespace RegisterService.DatabaseAccess
{
    public interface IUserRepository
    {
        Task<int> UserExistsByEmailAsync(string email);

        Task<int> CreateUserAsync(string name, string email, string password);

        Task<Player> GetUserByIdAsync(int id);

        Task<int> ActivateUserAccountAsync(int id);

        Task<int> RemoveUnactivatedUserAccountAsync(int id);
    }
}
