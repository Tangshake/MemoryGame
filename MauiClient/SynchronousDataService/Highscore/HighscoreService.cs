using MemoryGame.Model;
using MemoryGame.Model.Player;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MemoryGame.SynchronousDataService.Highscore;

public class HighscoreService : HttpClientBase, IHighscoreService
{
    public HighscoreService(IPlayerData plyerData) : base(plyerData)
    {
    }

    public async Task<bool> AddAsync(GameResultModelRequest gameResultModelRequest, string requestUri)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return false;

        string? jwtToken = await SecureStorage.Default.GetAsync("oauth_token");

        var httpContent = new StringContent(
            JsonSerializer.Serialize(gameResultModelRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await SendHttpPostAsync(requestUri, httpContent,"Bearer", jwtToken);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        return false;
    }

    public async Task<List<TopGamesResultsModelResponse>> GetAsync(int count, string requestUri)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return null;

        string? jwtToken = await SecureStorage.Default.GetAsync("oauth_token");

        var queryString = requestUri + $"/{count}";

        var response = await SendHttpGetAsync(queryString, "Bearer", jwtToken);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            var topGameResults = JsonSerializer.Deserialize<List<TopGamesResultsModelResponse>>(responseContent, options);

            return topGameResults;
        }

        return null;
    }
}
