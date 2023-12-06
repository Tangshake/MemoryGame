
using Dapper;
using LoginService.Entity;
using Npgsql;
using System.Diagnostics;

namespace LoginService.DatabaseAccess
{
    public class UserRepository(IConfiguration configuration) : IUserRepository
    {
        private readonly IConfiguration configuration = configuration;
        private readonly string ConnectionStringName = "MemoryGame";

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                var connectionString = configuration.GetConnectionString(ConnectionStringName);
                await using NpgsqlConnection connection = new(connectionString);

                var parameters = new { playerEmail = email };
                var query = " SELECT * FROM player WHERE email = @playerEmail";

                var user = await connection.QueryFirstOrDefaultAsync<User>(query, param: parameters);

                return user;
            }
            catch(Exception ex) 
            {
                Debug.WriteLine(ex);

            }

            return null;
        }
    }
}
