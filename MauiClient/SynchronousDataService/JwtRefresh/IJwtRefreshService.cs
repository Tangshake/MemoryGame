using MemoryGame.Model;
using MemoryGame.Model.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MemoryGame.SynchronousDataService.JwtRefresh
{
    public interface IJwtRefreshService
    {
        Task<RefreshTokenResponseModel?> RefreshJwtTokenAsync(HttpClient client, JsonSerializerOptions options, string url, RefreshTokenRequestModel refreshTokenRequestModel);
    }
}
