using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Model;

public class VerifyTokenModelRequest
{
    public int Id { get; set; }
    public string? Token { get; set; }    
}
