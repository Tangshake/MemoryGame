using SignalRHub.Model;

namespace SignalRHub.Repository.Jwt
{
    public interface IJwtRepository
    {
        Task<JwtSecret> GetJwtSecretAsync();
    }
}
