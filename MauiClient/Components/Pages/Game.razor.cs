using MemoryGame.Card;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using MemoryGame.LatestScore;
using MemoryGame.Model.Player;
using MemoryGame.Services;
using MemoryGame.Model;

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
        /// Set ON: when user tap the card for the first time. 
        /// Set OFF: when game ends. 
        /// </summary>
        private bool HasGameStarted { get; set; } = false;

        //Statistics
        private int NumberOfMoves { get; set; } = 0;

        // Latest user results - max 3 results
        private List<Score> latesScores = new(3);

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
                .Build();

                hubConnection.On<string>("UserConnectedToTheServer", UserConnectedToTheServer);
                hubConnection.On<string>("UserJoinedTheGame", UserJoinedTheGame);
                hubConnection.On<string, int, int>("UserScoreMessage", UserScoreMessage);

                await hubConnection.StartAsync();

            }catch(Exception e)
            {
                Debug.WriteLine(e);
            }

            // Get Jwt Token from Secure Storage
            var token = await SecureStorage.Default.GetAsync("oauth_token");

            // Set JwtExpirationTime
            JwtExpireTime = Tools.JwtBearerToken.JwtBearerDataExtractor.GetExpireDate(token!);
        }

        protected override async Task OnParametersSetAsync()
        {
            Debug.WriteLine($"User {PlayerData.Name} joined the game");
            await SendUserNameBySignalR(PlayerData.Name);

            StateHasChanged();
        }

        #region SignalR
        // Received when new player joins the server
        private void UserConnectedToTheServer(string message)
        {
            Debug.WriteLine($"{message}");
        }

        private async Task UserJoinedTheGame(string player)
        {
            ActivePlayerList.Insert(0, new JoinedUser { Name = player});

            await this.InvokeAsync(() => StateHasChanged());
        }


        // SignalR method invoked when received method from the hub
        private async Task UserScoreMessage(string player, int score, int timeAsMilliseconds)
        {
            latesScores.Insert(0, new Score(player, score, timeAsMilliseconds));

            await this.InvokeAsync(() => StateHasChanged());
            Debug.WriteLine($"Message from signalR: {player} score: {score} in: {TimeSpan.FromMilliseconds(time):hh\\:mm\\:ss}");
        }

        //SignalR method used to send message to the hub (invoking specific method on the Hub)
        private async Task SendUserNameBySignalR(string user)
        {
            try
            {
                await hubConnection.SendAsync("SendUserName", user);
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
                await hubConnection.SendAsync("SendMessage", user, score, time);
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

                // Stop the timer when game ends
                if (timer is not null && !IsGameEnabled)
                {
                    timer.Stop();
                    HasGameStarted = false;

                    // Add users score to the database
                    await GameResultRepository.AddGameResultAsync(new GameResultModelRequest() { Id = PlayerData.Id, Duration = time,  Moves = NumberOfMoves }, "https://localhost:7036/api/result");

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

        private bool CheckIfGameEnded() 
        {
            return Board.Count(x => x.Reversed) != 16 ? false : true;
        }
    }
}
