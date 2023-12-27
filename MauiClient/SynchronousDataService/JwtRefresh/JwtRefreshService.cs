using MemoryGame.Model;
using MemoryGame.Model.Post;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MemoryGame.SynchronousDataService.JwtRefresh
{
    public class JwtRefreshService : IJwtRefreshService
    {
        /// <summary>
        /// Refreshes Jwt token 
        /// </summary>
        /// <returns>Refreshed Jwt token and new refresh token</returns>
        public async Task<RefreshTokenResponseModel?> RefreshJwtTokenAsync(HttpClient httpClient, JsonSerializerOptions options, string url, RefreshTokenRequestModel refreshTokenRequestModel)
        {

            var requestContent = new StringContent(
                JsonSerializer.Serialize(refreshTokenRequestModel),
                Encoding.UTF8,
                "application/json"
            );

            // Send request to refresh jwt token
            var response = await httpClient.PostAsync(url, requestContent);

            if (!response.IsSuccessStatusCode)
                return null;

            // Read the reponse
            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseContent))
            {
                Debug.WriteLine($"Check if Jwt RefreshNeeded response: {responseContent}");
                return null;
            }

            // Deserialize response
            var refreshTokenResponseModel = JsonSerializer.Deserialize<RefreshTokenResponseModel>(responseContent, options);

            // Set both tokens
            //await SecureStorage.Default.SetAsync($"oauth_token_{tokensModel.UserId}", refreshTokenResponseModel.JwtToken);
            //await SecureStorage.Default.SetAsync($"refresh_{tokensModel.UserId}", refreshTokenResponseModel.RefreshToken);

            // Confirm that jwt was refreshed
            return refreshTokenResponseModel;

        }
    }
}
