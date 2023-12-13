using MemoryGame.Model;
using System.Text;
using System.Text.Json;

namespace MemoryGame.SynchronousDataService.Login
{
    public class LoginService : HttpClientBase, ILoginService
    {
        public async Task<LoginModelResponse> LoginUserAsync(LoginModelRequest loginModelRequest, string requestUri)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return new() { Success = false };


            var authenticationString = $"{loginModelRequest.Email}:{loginModelRequest.Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(authenticationString));

            var token = base64EncodedAuthenticationString;

            var response = await SendHttpPostAsync(requestUri, null, "Basic", token);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var loginModelResponse = JsonSerializer.Deserialize<LoginModelResponse>(responseContent, options);

                return loginModelResponse!;
            }

            return new() { Success = false };
        }
    }
}
