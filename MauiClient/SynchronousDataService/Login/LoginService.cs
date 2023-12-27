using MemoryGame.Model;
using MemoryGame.Model.Player;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MemoryGame.SynchronousDataService.Login
{
    public class LoginService : ApiBase, ILoginService
    {
        public async Task<LoginModelResponse> LoginUserAsync(LoginModelRequest loginModelRequest, string requestUri)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return new() { Success = false };

            // Basic authentication string
            var authenticationString = $"{loginModelRequest.Email}:{loginModelRequest.Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(authenticationString));

            //Set request header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

            var response = await client.PostAsync(requestUri, null);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var loginModelResponse = JsonSerializer.Deserialize<LoginModelResponse>(responseContent, jsonSerializerOptions);

                return loginModelResponse!;
            }

            return new() { Success = false };
        }
    }
}
