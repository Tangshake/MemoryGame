using MemoryGame.Bearer;
using MemoryGame.Model;
using MemoryGame.SynchronousDataService.JwtRefresh;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MemoryGame.SynchronousDataService.Highscore;

public class HighscoreService(IJwtRefreshService jwtRefreshService) : ApiBase, IHighscoreService
{
    public async Task<bool> AddAsync(GameResultModelRequest gameResultModelRequest, string requestUri)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return false;

        string? jwtToken = await SecureStorage.Default.GetAsync($"oauth_token_{gameResultModelRequest.Id}");
        string? refreshToken = await SecureStorage.Default.GetAsync($"refresh_{gameResultModelRequest.Id}");

        // Check if jwt needs to be refreshed
        var isRefreshNeeded = JwtInformationExtractor.Expired(jwtToken);

        if (isRefreshNeeded is null)
            return false;

        if(isRefreshNeeded == true)
        {
            var refreshTokenResponsModel = await jwtRefreshService.RefreshJwtTokenAsync(
                client,
                jsonSerializerOptions,
                Endpoints.Configuration.JwtRefreshEndpoint,
                new() { JwtToken = jwtToken, RefreshToken = refreshToken, userId = gameResultModelRequest.Id }
                );

            if (refreshTokenResponsModel is null)
                return false;

            // Set both tokens
            await SecureStorage.Default.SetAsync($"oauth_token_{gameResultModelRequest.Id}", refreshTokenResponsModel.JwtToken);
            await SecureStorage.Default.SetAsync($"refresh_{gameResultModelRequest.Id}", refreshTokenResponsModel.RefreshToken);

            // overwrite local token value
            jwtToken = refreshTokenResponsModel.JwtToken;
        }

        var httpContent = new StringContent(
            JsonSerializer.Serialize(gameResultModelRequest),
            Encoding.UTF8,
            "application/json"
        );

        //Add header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var response = await client.PostAsync(requestUri, httpContent);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        return false;
    }

    public async Task<List<TopGamesResultsModelResponse>> GetAsync(int count, int userId, string requestUri)
    {
        Debug.WriteLine("Get Highscore Top Results");

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return Enumerable.Empty<TopGamesResultsModelResponse>().ToList();

        string? jwtToken = await SecureStorage.Default.GetAsync($"oauth_token_{userId}");
        string? refreshToken = await SecureStorage.Default.GetAsync($"refresh_{userId}");

        // Check if jwt needs to be refreshed
        var isRefreshNeeded = JwtInformationExtractor.Expired(jwtToken);

        if (isRefreshNeeded is null)
            return Enumerable.Empty<TopGamesResultsModelResponse>().ToList();

        if (isRefreshNeeded == true)
        {
            var refreshTokenResponsModel = await jwtRefreshService.RefreshJwtTokenAsync(
                client,
                jsonSerializerOptions,
                Endpoints.Configuration.JwtRefreshEndpoint,
                new() { JwtToken = jwtToken, RefreshToken = refreshToken, userId = userId }
                );

            if (refreshTokenResponsModel is null)
                return Enumerable.Empty<TopGamesResultsModelResponse>().ToList();

            // Set both tokens
            await SecureStorage.Default.SetAsync($"oauth_token_{userId}", refreshTokenResponsModel.JwtToken);
            await SecureStorage.Default.SetAsync($"refresh_{userId}", refreshTokenResponsModel.RefreshToken);

            // overwrite local token value
            jwtToken = refreshTokenResponsModel.JwtToken;
        }

        //Add header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        var queryString = requestUri + $"/{count}";

        var response = await client.GetAsync(queryString);

        if (response.IsSuccessStatusCode)
        {
            Debug.WriteLine("Get Highscore Top Results: Success");
            var responseContent = await response.Content.ReadAsStringAsync();

            var topGameResults = JsonSerializer.Deserialize<List<TopGamesResultsModelResponse>>(responseContent, jsonSerializerOptions);

            Debug.WriteLine($"Get Highscore Top Results: {responseContent}");
            return topGameResults;
        }

        return null;
    }


}
