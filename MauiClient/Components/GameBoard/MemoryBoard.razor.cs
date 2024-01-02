using MemoryGame.Card;
using MemoryGame.Model.Game;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Components.GameBoard;

public partial class MemoryBoard : ComponentBase
{
    /// <summary>
    /// Game board size defined as NxN cards. 
    /// </summary>
    [Parameter]
    public int BoardSize { get; set; } = 4;

    [Parameter]
    public EventCallback<GameData> OnGameUpdate { get; set; }

    /// <summary>
    /// Game board containing even number of cards
    /// </summary>
    private MemoryCard[] GameBoard { get; set; }

    /// <summary>
    /// Timer used to measure the amount of time it took the user to finish the game.
    /// </summary>
    private IDispatcherTimer? timer;

    /// <summary>
    /// Timer interval
    /// </summary>
    private int timerInterval = 1000;

    /// <summary>
    /// Time has to pass before second card can be tapped
    /// </summary>
    private int tapDelay = 300;

    /// <summary>
    /// Flag that blocks car of being tapped
    /// </summary>
    private bool isTapEnabled = true;

    /// <summary>
    /// Number of moves (flipped cards) in the current game
    /// </summary>
    private int moves = 0;

    /// <summary>
    /// Amount of time passed in the game [s]
    /// </summary>
    private int gameTime;

    /// <summary>
    /// FLag that states if game ended or not
    /// </summary>
    private bool hasGameEnded = false;

    protected override void OnInitialized()
    {
        InitializeTimer();
        GameBoard = new MemoryCard[BoardSize * BoardSize];
        GenerateNewGameBoard();

        Debug.WriteLine("Initialize Memory Board");
    }

    /// <summary>
    /// MEthod to be called when we want to restart the game
    /// </summary>
    public void RestartTheGame()
    {
        timer?.Stop();

        GenerateNewGameBoard();
    }

    /// <summary>
    /// Timer initialization
    /// </summary>
    private void InitializeTimer()
    {
        timer = Application.Current!.Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(timerInterval);
        timer.Tick += Timer_Tick;
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        gameTime++;

        // Fire the event
        await OnGameUpdate.InvokeAsync(new() { HasGameStarted = true, Time = gameTime, Moves = moves, HasFinished = false }); ;
    }

    /// <summary>
    /// Generate new game cards on the board
    /// </summary>
    private void GenerateNewGameBoard()
    {
        Debug.WriteLine("Generating new game board");

        // Create array of available spots for the card
        List<int> spots = Enumerable.Range(0, BoardSize * BoardSize).ToList();

        // Create array of pairs of the cards - ToDo: Refactor
        List<int> cardNo = new();
        for(int i=0; i < spots.Count / 2; i++)
        {
            // Same card should appear twice in th egame board (on random spots)
            cardNo.Add(i);
            cardNo.Add(i);
        }

        Random random = new Random();

        // Generate board
        for (int i = 0; i < cardNo.Count; i++)
        {
            //Random card spot
            var max = spots.Count();
            var randSpot = random.Next(max);

            // Put the card in the spot
            GameBoard[spots[randSpot]] = new MemoryCard { SlotNumber = spots[randSpot], Number = cardNo[i] };

            // Remove spot from the list
            spots.RemoveAt(randSpot);
        }

        // Clear moves value
        moves = 0;

        // Reset time
        gameTime = 0;

    }

    /// <summary>
    /// Called whenever card is tapped
    /// </summary>
    /// <param name="index">Card index</param>
    private async Task OnCardTap(int index)
    {
        // First card that is tapped starts the game
        if (timer is not null && !timer.IsRunning)
        {
            timer.Start();
        }

        // Continue only if tap is enabled
        if (isTapEnabled)
        {
            // Disable flipping the card
            isTapEnabled = false;

            // Flip the card that was tapped. Already flipped card or matched one will be ignored returning false.
            var result = GameBoard[index].FilpTheCard();

            // If card is already flipped or already matched do nothing
            if (!result)
            {
                isTapEnabled = true;
                return;
            }

            // Increament number of player moves
            moves++;

            // Lets wait a bit to show user the card
            await Task.Delay(tapDelay);

            // If there are two cards reversed check if they matches
            if (GameBoard.Count(x => x.Reversed && x.Enabled == true) == 2)
            {
                var pairFound = LookForPair();

                // If pair was found it means game may be over
                if (pairFound)
                {
                    // Check if game has come to an end
                    hasGameEnded = CheckIfGameHasEnded();

                    if(hasGameEnded)
                    {
                        // Stop the timer
                        StopTheGame();

                        // Fire the event
                        await OnGameUpdate.InvokeAsync(new() { Time = gameTime, Moves = moves, HasFinished = true});

                    }
                }
            }

            // Enable tapping
            isTapEnabled = true;
        }
    }

    // Check if game should be ended - there are no cards to be flipped
    
    /// <summary>
    /// Check if there are two flipped cards that matches and makes a pair.
    /// If so disable those cards and leave them flipped
    /// </summary>
    /// <returns>Value that states if match was found or not</returns>
    private bool LookForPair()
    {
        // Get indexes of the flipped cards
        var indexes = GameBoard.Where(x => x.Reversed && x.Enabled == true).Select(x => x.SlotNumber).ToList();

        // When there are two cards - compare them
        if (indexes.Count() == 2 && GameBoard[indexes[0]].Number == GameBoard[indexes[1]].Number)
        {
            GameBoard[indexes[0]].Enabled = false;
            GameBoard[indexes[1]].Enabled = false;

            return true;
        }
        else
        {
            GameBoard[indexes[0]].Reversed = false;
            GameBoard[indexes[1]].Reversed = false;

            return false;
        }

    }

    /// <summary>
    /// Check if game has ended. There are no cards to be flipped
    /// </summary>
    /// <returns>Game result</returns>
    private bool CheckIfGameHasEnded()
    {
        return GameBoard.Count(x => x.Reversed) != 16 ? false : true;
    }

    /// <summary>
    /// Stop the game
    /// </summary>
    private void StopTheGame()
    {
        if (timer is not null && timer.IsRunning && hasGameEnded)
        {
            timer.Stop();
        }
    }
}
