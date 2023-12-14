using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Model.Player
{
    public interface IPlayerData
    {
        int Id { get; set; }
        string Email { get; set; }
        string Name { get; set; }
        DateTime JoinTime { get; set; }
    }
}
