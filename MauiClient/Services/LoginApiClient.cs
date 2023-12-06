using MemoryGame.Model;
using System.Diagnostics;
using System.Text;
using System.Text.Json;


namespace MemoryGame.Services;

public class LoginApiClient : ILoginApiClient
{
    public async Task<LoginModelResponse> LoginUserAsync(LoginModelRequest loginModelRequest, string requestUri)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return new() { Success = false };

        using HttpClient httpClient = new HttpClient();

        var authenticationString = $"{loginModelRequest.Email}:{loginModelRequest.Password}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(authenticationString));

        httpClient.DefaultRequestHeaders.Add("Authorization", "Basic "+base64EncodedAuthenticationString);

        var response = await httpClient.PostAsync(requestUri, null);

        Debug.WriteLine($"Result: {response.StatusCode}");
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            var loginModelResponse = JsonSerializer.Deserialize<LoginModelResponse>(responseContent, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            
            Debug.WriteLine(responseContent);

            return loginModelResponse!;
        }

        return new() { Success = false };
    }
    
}
