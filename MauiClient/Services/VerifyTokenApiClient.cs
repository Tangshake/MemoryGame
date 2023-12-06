using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemoryGame.Services
{
    public class VerifyTokenApiClient : IVerifyTokenApiClient
    {
        public async Task<bool> VerifyTokenAsync(VerifyTokenModelRequest verifyCodeModelRequest, string requestUri)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                return false;

            var requestContent = new StringContent(
                JsonSerializer.Serialize(verifyCodeModelRequest),
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

                return true;
            }

            return false;
        }
    }
}
