using LoginService.AuthTokens.Model;

namespace LoginService.DatabaseAccess
{
    public interface ITokenRepository
    {
        Task<string> GetJwtSecKeyAsync();

        Task<int> CreateRefreshTokenAsync(int userId, RefreshToken refreshToken);

        Task<string> GetRefreshTokenAsync(int userId);
    }
}
