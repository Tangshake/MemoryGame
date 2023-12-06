using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Model;

public class RegisterModel
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, MinLength(3), MaxLength(20)]
    public string? Name { get; set; }

    [Required, MinLength(3)]
    public string? Password{ get; set; }

    [Required, Compare(nameof(RegisterModel.Password))]
    public string? RePassword { get; set; }
}
