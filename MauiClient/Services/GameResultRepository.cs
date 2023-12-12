using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemoryGame.Services;

public class GameResultRepository : IGameResultRepository
{
    readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

    public async Task<bool> AddGameResultAsync(GameResultModelRequest gameResultModelRequest, string requestUri)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return false;

        var httpContent = new StringContent(
            JsonSerializer.Serialize(gameResultModelRequest),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            using HttpClient httpClient = new HttpClient();
            var response = await httpClient.PostAsync(requestUri, httpContent);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return false;
    }

    public async Task<List<TopGamesResultsModelResponse>> GetTopResults(int count, string requestUri)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return null;

        string oauthToken = await SecureStorage.Default.GetAsync("oauth_token");

        var queryString = requestUri + $"/{count}";

        try
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oauthToken);

            var response = await httpClient.GetAsync(queryString);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var topGameResults = JsonSerializer.Deserialize<List<TopGamesResultsModelResponse>>(responseContent, options);
            
                return topGameResults;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        return null;
    }
}
