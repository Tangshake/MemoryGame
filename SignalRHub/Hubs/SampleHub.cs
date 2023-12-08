using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Numerics;

namespace SignalRHub.Hubs;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class SampleHub : Hub
{
    // Invoked (by Client) when connection is opened by the client
    public override async Task OnConnectedAsync()
    {
        // Notice others that new player has joined the game
        await Clients.Others.SendAsync("UserConnectedToTheServer", $"New player has joined the game.");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Trace.WriteLine($"User with connection id: {Context.ConnectionId} has disconnected.");
        await base.OnDisconnectedAsync(exception);
    }

    // Invoked (by Client) when player joins the game
    public async Task SendUserName(string player)
    {
        // Send player name to other players
        await Clients.All.SendAsync("UserJoinedTheGame", player);
    }

    // Invoked (by Client) when player finish his game
    public async Task SendMessage(string player, int score, int timeAsMilliseconds)
    {
        // Send score to other players
        await Clients.All.SendAsync("UserScoreMessage", player, score, timeAsMilliseconds);
    }

}
