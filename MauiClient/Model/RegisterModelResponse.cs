using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Model
{
    public class RegisterModelResponse
    {
        public int Id { get; set; }
        public bool RegisterSuccess { get; set; } = false;
        public String? Message { get; set; }
    }
}
