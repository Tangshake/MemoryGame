using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace MemoryGame.SynchronousDataService
{
    public abstract class HttpClientBase
    {
        private readonly HttpClient httpClient = new HttpClient();

        protected readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

        public virtual async Task<HttpResponseMessage> SendHttpPostAsync(string url, StringContent content, string scheme, string token)
        {
            if(scheme is not null)
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);

            try
            {
                return await httpClient.PostAsync(url, content);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<HttpResponseMessage> SendHttpGetAsync(string url, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                return await httpClient.GetAsync(url);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<HttpResponseMessage> RefreshTokenIfNeeded(string url, StringContent content, string token)
        {
            try
            {

                return await httpClient.PostAsync(url, content);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
