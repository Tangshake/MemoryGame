using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Model.Game;

public class GameData
{
    public int Time { get; set; }
    public int Moves { get; set; }

    public bool HasGameStarted { get; set; } = false;
    public bool HasFinished { get; set; }
}
