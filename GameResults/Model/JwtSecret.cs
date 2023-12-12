using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace GameResults.Model;

[Table("jwt")]
public class JwtSecret
{
    [Description("jwt_key")]
    public required string jwt_key { get; set; }
}
