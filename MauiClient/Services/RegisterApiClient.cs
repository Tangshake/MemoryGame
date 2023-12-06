using MemoryGame.Model;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MemoryGame.Services
{
    public class RegisterApiClient : IRegisterApiClient
    {
        public async Task<RegisterModelResponse> RegisterUserAsync(RegisterModelRequest registerModelRequest, string requestUri)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return new RegisterModelResponse { Id = -1, Message = "No internet connection!", RegisterSuccess = false };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(registerModelRequest),
                Encoding.UTF8,
                "application/json"
                );

            using HttpClient httpClient = new HttpClient();
            var response = await httpClient.PostAsync(requestUri, requestContent);

            Debug.WriteLine($"Success: {response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(responseContent);

                return JsonSerializer.Deserialize<RegisterModelResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return new RegisterModelResponse { Id = -1, Message = "Unknown error", RegisterSuccess = false };
        }
    }
}
