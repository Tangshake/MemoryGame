using MemoryGame.Bearer;
using MemoryGame.Model;
using MemoryGame.Model.Player;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace MemoryGame.SynchronousDataService
{
    public abstract class HttpClientBase
    {
#if DEBUG
        private readonly HttpClient httpClient = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });
#else
        private readonly HttpClient httpClient = new HttpClient(new HttpClientHandler();
#endif

        protected readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        protected IPlayerData playerData;

        protected HttpClientBase(IPlayerData playerData)
        {
            this.playerData = playerData;
        }



        public virtual async Task<HttpResponseMessage> SendHttpPostAsync(string url, StringContent content, string scheme, string token)
        {
            // If endpoint address is null return 
            if (string.IsNullOrEmpty(url))
                return new HttpResponseMessage(HttpStatusCode.UnprocessableContent);


            if (!string.IsNullOrEmpty(scheme) && scheme.Equals("Bearer"))
            {
                var refreshed = await RefreshJwtIfNeeded();
                if(refreshed)
                    token = await SecureStorage.Default.GetAsync($"oauth_token_{playerData.Id}");
            }

            
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);

            try
            {
                return await httpClient.PostAsync(url, content);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public virtual async Task<HttpResponseMessage> SendHttpGetAsync(string url, string scheme, string token)
        {
            // If endpoint address is null return 
            if (string.IsNullOrEmpty(url))
                return new HttpResponseMessage(HttpStatusCode.UnprocessableContent);


            if (!string.IsNullOrEmpty(scheme) && scheme.Equals("Bearer"))
            {
                var refreshed = await RefreshJwtIfNeeded();
                if (refreshed)
                    token = await SecureStorage.Default.GetAsync($"oauth_token_{playerData.Id}");
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);

            try
            {
                return await httpClient.GetAsync(url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Refreshes Jwt token if it expires
        /// </summary>
        /// <returns>Value whether token has been refreshed or not</returns>
        public virtual async Task<bool> RefreshJwtIfNeeded()
        {
            Debug.WriteLine("Check if Jwt RefreshNeeded");

            // Get jwt token from the 
            var jwtToken = await SecureStorage.Default.GetAsync($"oauth_token_{playerData.Id}");
            var refreshToken = await SecureStorage.Default.GetAsync($"refresh_{playerData.Id}");

            // Check if jwt needs to be refreshed
            var isRefreshNeeded = JwtInformationExtractor.Expired(jwtToken);

            if(isRefreshNeeded)
            {

                var requestContent = new StringContent(
                JsonSerializer.Serialize(new RefreshTokenRequestModel() {userId = playerData.Id, JwtToken = jwtToken, RefreshToken = refreshToken}),
                Encoding.UTF8,
                "application/json"
                );

                // Send request to refresh jwt token
                var response = await httpClient.PostAsync(Endpoints.Configuration.JwtRefreshEndpoint, requestContent);

                // Read the reponse
                var responseContent = await response.Content.ReadAsStringAsync();

                if(string.IsNullOrEmpty(responseContent))
                    Debug.WriteLine($"Check if Jwt RefreshNeeded response: {responseContent}");

                // Deserialize response
                var refreshTokenResponseModel = JsonSerializer.Deserialize<RefreshTokenResponseModel>(responseContent, options);

                // Set both tokens
                await SecureStorage.Default.SetAsync($"oauth_token_{playerData.Id}", refreshTokenResponseModel.JwtToken);
                await SecureStorage.Default.SetAsync($"refresh_{playerData.Id}", refreshTokenResponseModel.RefreshToken);

                Debug.WriteLine($"Check if Jwt RefreshNeeded: {isRefreshNeeded}");
                // Confirm that jwt was refreshed
                return true;

            }

            Debug.WriteLine($"Is refresh needed: {isRefreshNeeded}");
            return false;
        }
    }
}
