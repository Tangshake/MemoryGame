using GameResults.Model;

namespace GameResults.DatabasAccess
{
    public interface IJwtRepository
    {
        Task<JwtSecret> GetJwtSecretAsync();
    }
}
