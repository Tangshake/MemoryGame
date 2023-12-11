using MemoryGame.Card;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using MemoryGame.Model.Player;
using MemoryGame.Services;
using MemoryGame.Model;
using MemoryGame.SignalR.Settings;

namespace MemoryGame.Components.Pages
{
    public partial class Game : ComponentBase
    {
        [Inject]
        PlayerData PlayerData { get; set; }

        [Inject]
        IGameResultRepository GameResultRepository { get; set; }

        [Parameter]
        public string? Name { get; set; }

        private HubConnection? hubConnection;

        private MemoryCard[] Board { get; set; } = new MemoryCard[16];

        private bool IsTapEnabled { get; set; } = true;

        private bool IsGameEnabled { get; set; } = true;

        /// <summary>
        /// Flag:
        /// Is client connected to signalR Hub
        /// </summary>
        private bool IsSignalRHubAvailable{ get; set; } = false;

        /// <summary>
        /// Flag:
        /// Set ON: when user tap the card for the first time. 
        /// Set OFF: when game ends. 
        /// </summary>
        private bool HasGameStarted { get; set; } = false;

        //Statistics
        private int NumberOfMoves { get; set; } = 0;

        // Latest user results - max 3 results
        private List<TopGamesResultsModelResponse> Highscore = new();

        // Nicks of 3 last users that joined the server
        private List<JoinedUser> ActivePlayerList = new(3);
        
        /// <summary>
        /// Jwt Expiration time as a DateTime
        /// </summary>
        private DateTime JwtExpireTime { get; set; }

        private IDispatcherTimer? timer;
        private int time;

        protected override async Task OnInitializedAsync()
        {
            InitializeTimer();
            GenerateNewBoard();
            await RefreshHighscore();

            try
            {
                string oauthToken = await SecureStorage.Default.GetAsync("oauth_token");

                var baseUrl = DeviceInfo.Platform == DevicePlatform.Android ? 
                        "http://10.0.2.2:5106" : "https://localhost:5106";

                /* Initialize SignalR HubConnection */
                hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{baseUrl}/game-hub", options => {
                        options.AccessTokenProvider = () => Task.FromResult(oauthToken);
                    })
                .WithAutomaticReconnect(new SignalRRetryPolicy())
                .Build();

                hubConnection.Closed += HubConnection_Closed;
                hubConnection.Reconnecting += HubConnection_Reconnecting;
                hubConnection.Reconnected += HubConnection_Reconnected;

                hubConnection.On<string>("UserConnectedToTheServer", UserConnectedToTheServer);
                hubConnection.On<string>("UserJoinedTheGame", UserJoinedTheGame);
                hubConnection.On<string, int, int>("UserScoreMessage", UserScoreMessage);

                await hubConnection.StartAsync();

            }
            catch(HttpRequestException ex)
            {
                Debug.WriteLine(ex);
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
            }

            // Get Jwt Token from Secure Storage
            var token = await SecureStorage.Default.GetAsync("oauth_token");

            // Set JwtExpirationTime
            JwtExpireTime = Tools.JwtBearerToken.JwtBearerDataExtractor.GetExpireDate(token!);

            // Set SignlR Hub connection
            IsSignalRHubAvailable = true;
        }

        private Task HubConnection_Reconnected(string? arg)
        {
            IsSignalRHubAvailable = true;
            this.InvokeAsync(() => StateHasChanged());
            return Task.CompletedTask;
        }

        private Task HubConnection_Reconnecting(Exception? arg)
        {
            IsSignalRHubAvailable = false;
            this.InvokeAsync(() => StateHasChanged());
            return Task.CompletedTask;
        }

        private Task HubConnection_Closed(Exception? arg)
        {
            IsSignalRHubAvailable = false;
            return Task.CompletedTask;
        }

        protected override async Task OnParametersSetAsync()
        {
            Debug.WriteLine($"User {PlayerData.Name} joined the game");
            await SendUserNameBySignalR(PlayerData.Name);

            StateHasChanged();
        }

        #region SignalR
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
            Debug.WriteLine($"Message from signalR: {player} score: {score} in: {TimeSpan.FromMilliseconds(time):hh\\:mm\\:ss}");
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

        private void InitializeTimer()
        {
            timer = Application.Current!.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            time += 1000;
            StateHasChanged();
        }

        private void GenerateNewBoard()
        {
            //EnableGame
            IsGameEnabled = true;
            time = 0;

            //Reset statistics
            NumberOfMoves = 0;

            //C# 12 - new fancy way
            List<int> spots = [ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 ];
            List<int> cardNo = [1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9];

            Random random = new Random();

            // Generate board - 16spots
            for(int i=0; i < 16; i++)
            {
                //Random card spot
                var max = spots.Count();
                var randSpot = random.Next(max);

                // Put the card in the spot
                Board[spots[randSpot]] = new MemoryCard { SlotNumber = spots[randSpot], Number = cardNo[i] };

                // Remove spot from the list
                spots.RemoveAt(randSpot);
            }
        }

        public async Task OnCardTap(int cardNumberTapped)
        {
            // Start the timer at first tap/click
            if (timer is not null && IsGameEnabled && !timer.IsRunning)
            {
                HasGameStarted = true;
                timer.Start();
            }

            if (IsTapEnabled)
            {
                Debug.WriteLine($"Card Index: {cardNumberTapped} Number: {Board[cardNumberTapped].Number}");

                //Check if card can be reversed
                var result = Board[cardNumberTapped].FilpTheCard();

                // If there are two cards reversed check if they match
                if (Board.Count(x => x.Reversed && x.Enabled == true) == 2)
                {
                    NumberOfMoves++;
                    await Check();
                }

                IsGameEnabled = !CheckIfGameEnded();
                
                // Hardcoded: placed only to end the game after the first card tap
                IsGameEnabled = false;

                // Stop the timer when game ends
                if (timer is not null && !IsGameEnabled)
                {
                    timer.Stop();
                    HasGameStarted = false;

                    //Add result to the database
                    await AddGameResultToDatabase(new GameResultModelRequest() { Id = PlayerData.Id, Duration = time, Moves = NumberOfMoves });

                    // Refresh score
                    await RefreshHighscore();

                    // Inform others about user score
                    await SendUserScoreBySignalR(Name, NumberOfMoves, time);
                }

                Debug.WriteLine($"Game should be ended: {IsGameEnabled}");
            }
        }

        private async Task Check()
        {
            IsTapEnabled = false;
            await Task.Delay(500);
            var indexes = Board.Where(x => x.Reversed && x.Enabled == true).Select(x=>x.SlotNumber).ToList();

            if(indexes.Count() == 2 && Board[indexes[0]].Number == Board[indexes[1]].Number)
            {
                Board[indexes[0]].Enabled = false;
                Board[indexes[1]].Enabled = false;
            }
            else
            {
                Board[indexes[0]].Reversed = false;
                Board[indexes[1]].Reversed = false;
            }

            IsTapEnabled = true;
        }

        private async Task RefreshHighscore()
        {
            var result = await GameResultRepository.GetTopResults(3, "https://localhost:7036/api/result");

            if (result is not null)
            {
                Highscore.Clear();
                Highscore.AddRange(result);
            }
        }
        
        private async Task AddGameResultToDatabase(GameResultModelRequest gameResultModelRequest)
        {
            // Add user score to the database
            await GameResultRepository.AddGameResultAsync(gameResultModelRequest, "https://localhost:7036/api/result");
        }

        private bool CheckIfGameEnded() 
        {
            return Board.Count(x => x.Reversed) != 16 ? false : true;
        }
       
    }
}
