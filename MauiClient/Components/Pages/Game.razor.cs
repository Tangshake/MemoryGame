using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using MemoryGame.Model.Player;
using MemoryGame.Model;
using MemoryGame.SignalR.Settings;
using MemoryGame.Model.Game;
using MemoryGame.Components.GameBoard;
using MemoryGame.SynchronousDataService.Highscore;
using Microsoft.Extensions.Logging;

namespace MemoryGame.Components.Pages
{
    public partial class Game : ComponentBase
    {
        MemoryBoard? MemoryGameComponentReference { get; set; }

        [Inject]
        IPlayerData? PlayerData { get; set; }

        [Inject]
        IHighscoreService? HighscoreService { get; set; }

        [Parameter]
        public string? Name { get; set; }

        /// <summary>
        /// Timer interval
        /// </summary>
        private int timerInterval = 1000;

        private HubConnection? hubConnection;

        /// <summary>
        /// Flag:
        /// Is client connected to signalR Hub
        /// </summary>
        private bool IsSignalRHubAvailable{ get; set; } = false;

        /// <summary>
        /// Game statistics
        /// </summary>
        private GameData GameData { get; set; } = new();

        /// <summary>
        /// Highscore list
        /// </summary>
        private List<TopGamesResultsModelResponse> Highscore = new();

        // Nicks of 3 last users that joined the server
        private List<JoinedUser> ActivePlayerList = new(3);
        
        /// <summary>
        /// Jwt Expiration time as a DateTime
        /// </summary>
        private DateTime JwtExpireTime { get; set; }

        private IDispatcherTimer? timer;

        protected override async Task OnInitializedAsync()
        {
            await RefreshHighscore();

            await ConnectToSignalRHub();

            // Get Jwt Token from Secure Storage
            var token = await SecureStorage.Default.GetAsync($"oauth_token_{PlayerData.Id}");

            // Set JwtExpirationTime
            JwtExpireTime = Tools.JwtBearerToken.JwtBearerDataExtractor.GetExpireDate(token!);

            // Set SignlR Hub connection
            IsSignalRHubAvailable = true;

            InitializeTimer();

            Debug.WriteLine(PlayerData.ToString());
        }

        private async Task ConnectToSignalRHub()
        {
            await RefreshHighscore();

            try
            {
                string oauthToken = await SecureStorage.Default.GetAsync($"oauth_token_{PlayerData.Id}");

                var baseUrl = DeviceInfo.Platform == DevicePlatform.Android ?
                        "https://10.0.2.2:5106" : "https://localhost:5106";

                /* Initialize SignalR HubConnection */
                hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{baseUrl}/game-hub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(oauthToken);

#if DEBUG
                        options.HttpMessageHandlerFactory = (message) =>
                        {
                            if (message is HttpClientHandler clientHandler)
                                // always verify the SSL certificate
                                clientHandler.ServerCertificateCustomValidationCallback +=
                                    (sender, certificate, chain, sslPolicyErrors) => { return true; };
                            return message;
                        };

                        options.WebSocketConfiguration = wsc => wsc.RemoteCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;

#endif
                    })
                    .ConfigureLogging(logging => {
                        // Set the log level of signalr stuffs
                        logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    })
                    //.WithAutomaticReconnect(new SignalRRetryPolicy())
                    .WithAutomaticReconnect([TimeSpan.FromSeconds(20)])
                    .Build();

                hubConnection.Closed += HubConnection_Closed;
                hubConnection.Reconnecting += HubConnection_Reconnecting;
                hubConnection.Reconnected += HubConnection_Reconnected;

                hubConnection.On<string>("UserConnectedToTheServer", UserConnectedToTheServer);
                hubConnection.On<string>("UserJoinedTheGame", UserJoinedTheGame);
                hubConnection.On<string, int, int>("UserScoreMessage", UserScoreMessage);

                await hubConnection.StartAsync();

                // Set SignlR Hub connection
                IsSignalRHubAvailable = true;

                await InvokeAsync(() => StateHasChanged());
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// Timer initialization
        /// </summary>
        private void InitializeTimer()
        {
            timer = Application.Current!.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            timer.Tick += Timer_Tick;

            //timer.Start();
        }

        /// <summary>
        /// Timer event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Timer_Tick(object? sender, EventArgs e)
        {
            Debug.WriteLine(JwtExpireTime.Subtract(DateTime.Now));
        }

        #region SignalR

        private Task HubConnection_Reconnected(string? arg)
        {
            IsSignalRHubAvailable = true;
            this.InvokeAsync(() => StateHasChanged());
            Debug.WriteLine("SignalR->Reconnected");
            return Task.CompletedTask;
        }

        private Task HubConnection_Reconnecting(Exception? arg)
        {
            IsSignalRHubAvailable = false;
            Debug.WriteLine("SignalR->Reconnecting");
            this.InvokeAsync(() => StateHasChanged());
            return Task.CompletedTask;
        }

        private async Task HubConnection_Closed(Exception? arg)
        {
            Debug.WriteLine("SignalR->Closed");
            IsSignalRHubAvailable = false;
            
            if (hubConnection is not null)
            {
                hubConnection.Closed -= HubConnection_Closed;
                hubConnection.Reconnecting -= HubConnection_Reconnecting;
                hubConnection.Reconnected -= HubConnection_Reconnected;
            }

            await ConnectToSignalRHub();
        }

        protected override async Task OnParametersSetAsync()
        {
            Debug.WriteLine($"User {PlayerData.Name} joined the game");
            await SendUserNameBySignalR(PlayerData.Name);

            StateHasChanged();
        }

        /// <summary>
        /// Invoked when user joins SignalR hub
        /// </summary>
        /// <param name="message">Message from the hub</param>
        private void UserConnectedToTheServer(string message)
        {
            Debug.WriteLine($"{message}");
        }

        /// <summary>
        /// Invoked when user joins the game
        /// </summary>
        /// <param name="player">Name of the player</param>
        /// <returns>Task</returns>
        private async Task UserJoinedTheGame(string player)
        {
            ActivePlayerList.Insert(0, new JoinedUser { Name = player});

            await this.InvokeAsync(() => StateHasChanged());
        }

        // SignalR method invoked when received method from the hub
        private async Task UserScoreMessage(string player, int score, int timeAsMilliseconds)
        {
            await RefreshHighscore();
            await this.InvokeAsync(() => StateHasChanged());
            Debug.WriteLine($"Message from signalR: {player} score: {score} in: {TimeSpan.FromSeconds(GameData.Time):hh\\:mm\\:ss}");
        }

        //SignalR method used to send message to the hub (invoking specific method on the Hub)
        private async Task SendUserNameBySignalR(string user)
        {
            try
            {
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    await hubConnection.SendAsync("SendUserName", user);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        //SignalR method used to send message to the hub (invoking specific method on the Hub)
        private async Task SendUserScoreBySignalR(string user, int score, int time)
        {
            try
            {
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    await hubConnection.SendAsync("SendMessage", user, score, time);
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
            }
        }
        #endregion

        /// <summary>
        /// Refeshes ingame highscore
        /// </summary>
        /// <returns></returns>
        private async Task RefreshHighscore()
        {
            var result = await HighscoreService.GetAsync(3, PlayerData.Id, Endpoints.Configuration.HighscoreEndpoint);
            
            if (result is not null)
            {
                Highscore.Clear();
                Highscore.AddRange(result);

                // Get Jwt Token from Secure Storage
                var token = await SecureStorage.Default.GetAsync($"oauth_token_{PlayerData.Id}");

                // Set JwtExpirationTime
                JwtExpireTime = Tools.JwtBearerToken.JwtBearerDataExtractor.GetExpireDate(token!);

                Debug.WriteLine(JwtExpireTime);
            }
        }
        
        /// <summary>
        /// Adds game results to the database
        /// </summary>
        /// <param name="gameResultModelRequest"></param>
        /// <returns>Task</returns>
        private async Task AddGameResultToDatabase(GameResultModelRequest gameResultModelRequest)
        {
            // Add user score to the database
            await HighscoreService.AddAsync(gameResultModelRequest, Endpoints.Configuration.HighscoreEndpoint);
        }

        /// <summary>
        /// Event subscribed from the game component. Called in intervals /1s
        /// </summary>
        /// <param name="data">Game statistics</param>
        /// <returns>Task</returns>
        private async Task OnGameUpdate(GameData data)
        {
            GameData.Moves = data.Moves;
            GameData.Time = data.Time;
            GameData.HasFinished = data.HasFinished;
            GameData.HasGameStarted = data.HasGameStarted;

            // If the game has ended
            if (GameData.HasFinished)
            {
                // Add result to the database
                await AddGameResultToDatabase(new()
                {
                    Id = PlayerData.Id,
                    Time = DateTime.Now,
                    Duration = GameData.Time,
                    Moves = GameData.Moves
                });

                // Refresh Highscore
                await RefreshHighscore();

                // Notice other players about my result
                await SendUserScoreBySignalR(PlayerData.Name, GameData.Moves, GameData.Time);
            }
        }

        /// <summary>
        /// Method to be called when we want to restart the game
        /// </summary>
        private void RestartTheGame()
        {
            MemoryGameComponentReference?.RestartTheGame();
            GameData = new();
        }
    }
}
