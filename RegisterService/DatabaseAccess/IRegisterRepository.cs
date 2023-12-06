using RegisterService.Entity;

namespace RegisterService.DatabaseAccess
{
    public interface IRegisterRepository
    {
        Task<int> SetUserActivationAsync(int userId, int tokenLifetime, string token);

        Task<Activation> GetUserActivationInformationAsync(int userId);

        Task<int> RemoveActivationEntryAsync(int userId);
    }
}
