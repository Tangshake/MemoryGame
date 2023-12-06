
using Dapper;
using Npgsql;
using SignalRHub.Model;

namespace SignalRHub.Repository.Jwt
{
    public class JwtRepository(IConfiguration configuration) : IJwtRepository
    {
        private readonly string ConnectionStringName = "MemoryGame";
        public async Task<JwtSecret> GetJwtSecretAsync()
        {
            try
            {
                var connectionString = configuration.GetConnectionString(ConnectionStringName);
                var connection = new NpgsqlConnection(connectionString);

                var query = "SELECT * FROM jwt";

                var result = await connection.QueryFirstOrDefaultAsync<JwtSecret>(query);

                if(result is null)
                    return new JwtSecret() { jwt_key = "" };

                return result;
            }
            catch(Exception e)
            {

            }

            return new JwtSecret() { jwt_key = "" };
        }
    }
}
