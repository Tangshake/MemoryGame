using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Model.Post
{
    public class RefreshTokenModel
    {
        public int UserId { get; set; }
        public string Bearer { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
