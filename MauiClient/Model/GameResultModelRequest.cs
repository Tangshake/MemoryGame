using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Model;

public class GameResultModelRequest
{
    public int Id { get; set; }
    public int Duration { get; set; }
    public int Moves { get; set; }
    public DateTime Time { get; set; } = DateTime.Now;
}
