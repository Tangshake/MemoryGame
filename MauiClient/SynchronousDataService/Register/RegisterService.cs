using MemoryGame.Model;
using MemoryGame.Model.Player;
using System.Text;
using System.Text.Json;

namespace MemoryGame.SynchronousDataService.Register
{
    public class RegisterService : HttpClientBase, IRegisterService
    {
        public RegisterService(IPlayerData plyerData) : base(plyerData)
        {
        }

        public async Task<RegisterModelResponse> RegisterUserAsync(RegisterModelRequest registerModelRequest, string requestUri)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return new RegisterModelResponse { Id = -1, Message = "No internet connection!", RegisterSuccess = false };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(registerModelRequest),
                Encoding.UTF8,
                "application/json"
                );

            var response = await SendHttpPostAsync(requestUri, requestContent, null, null);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<RegisterModelResponse>(responseContent, options);
            }

            return new RegisterModelResponse { Id = -1, Message = "Unknown error", RegisterSuccess = false };
        }
    }
}
