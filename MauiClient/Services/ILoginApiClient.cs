using MemoryGame.Model;

namespace MemoryGame.Services;

public interface ILoginApiClient
{
    Task<LoginModelResponse> LoginUserAsync(LoginModelRequest loginModelRequest, string requestUri);

}
