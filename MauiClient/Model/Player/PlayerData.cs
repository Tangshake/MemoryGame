using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Model.Player;

public class PlayerData : IPlayerData
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime JoinTime { get; set; } = DateTime.Now;
}
