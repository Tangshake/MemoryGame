using MemoryGame.Model;

namespace MemoryGame.SynchronousDataService.Login
{
    public interface ILoginService
    {
        Task<LoginModelResponse> LoginUserAsync(LoginModelRequest loginModelRequest, string requestUri);
    }
}
