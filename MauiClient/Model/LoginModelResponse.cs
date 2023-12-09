using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Model
{
    public class LoginModelResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string JwtToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTime Created { get; set; }
        public DateTime Expired { get; set; }
        public required bool Success { get; set; } = false;
    }
}
