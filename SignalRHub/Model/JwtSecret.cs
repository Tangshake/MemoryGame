using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalRHub.Model
{
    [Table("jwt")]
    public class JwtSecret
    {
        [Description("jwt_key")]
        public required string jwt_key { get; set; }
    }
}
