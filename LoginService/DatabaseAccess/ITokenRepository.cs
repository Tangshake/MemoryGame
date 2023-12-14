using LoginService.Entity;

namespace LoginService.DatabaseAccess
{
    public interface ITokenRepository
    {
        Task<string> GetJwtSecKeyAsync();

        Task<int> CreateRefreshTokenAsync(int userId, RefreshToken refreshToken);

        Task<RefreshToken> GetRefreshTokenAsync(int userId);
    }
}
