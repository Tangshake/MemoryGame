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

        string? jwtToken = await SecureStorage.Default.GetAsync($"oauth_token_{playerData.Id}");

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
        Debug.WriteLine("Get Highscore Top Results");

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return null;

        string? jwtToken = await SecureStorage.Default.GetAsync($"oauth_token_{playerData.Id}");

        var queryString = requestUri + $"/{count}";

        var response = await SendHttpGetAsync(queryString, "Bearer", jwtToken);

        if (response.IsSuccessStatusCode)
        {
            Debug.WriteLine("Get Highscore Top Results: Success");
            var responseContent = await response.Content.ReadAsStringAsync();

            var topGameResults = JsonSerializer.Deserialize<List<TopGamesResultsModelResponse>>(responseContent, options);

            Debug.WriteLine($"Get Highscore Top Results: {responseContent}");
            return topGameResults;
        }

        return null;
    }
}
