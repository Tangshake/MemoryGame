
using Dapper;
using Npgsql;
using RegisterService.Entity;
using System.Diagnostics;
using System.Xml.Linq;

namespace RegisterService.DatabaseAccess
{
    public class UserRepository(IConfiguration configuration) : IUserRepository
    {
        private readonly IConfiguration configuration = configuration;
        private readonly string ConnectionStringName = "MemoryGame";

        public async Task<int> ActivateUserAccountAsync(int id)
        {
            try
            {
                var connectionsString = configuration.GetConnectionString(ConnectionStringName);
                await using NpgsqlConnection connection = new(connectionsString);

                var parameters = new { id = id};

                var query = @"UPDATE player SET active = 1 WHERE id = @id";

                var result = await connection.ExecuteAsync(query, param: parameters);

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return -1;

            }
        }

        public async Task<int> CreateUserAsync(string name, string email, string password)
        {
            try
            {
                var connectionsString = configuration.GetConnectionString(ConnectionStringName);
                await using NpgsqlConnection connection = new(connectionsString);

                var parameters = new { name = name, email = email, password = password };

                var query = @"INSERT INTO player VALUES (default, @name, @email, @password) RETURNING id";

                var result = await connection.ExecuteScalarAsync<int>(query, param: parameters);

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return -1;

            }
        }

        public async Task<Player> GetUserByIdAsync(int id)
        {
            try
            {
                var connectionString = configuration.GetConnectionString(ConnectionStringName);
                var connection = new NpgsqlConnection(connectionString);

                var parameters = new { id = id };
                var query = "SELECT * FROM player WHERE id = @id";

                var result = await connection.QueryFirstOrDefaultAsync<Player>(query, param: parameters);

                return result!;
            }
            catch(Exception)
            {
                return null!;
            }
        }

        public async Task<int> RemoveUnactivatedUserAccountAsync(int id)
        {
            try
            {
                var connectionString = configuration.GetConnectionString(ConnectionStringName);
                var connection = new NpgsqlConnection(connectionString);

                var parameters = new { id = id };
                var query = "DELETE FROM player WHERE id = @id AND active = 0";

                var result = await connection.ExecuteAsync(query, param: parameters);

                return result!;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<int> UserExistsByEmailAsync(string email)
        {
            try
            {
                var connectionString = configuration.GetConnectionString(ConnectionStringName);
                await using NpgsqlConnection connection = new(connectionString);

                var parameters = new { playerEmail = email };
                var query = " SELECT * FROM player WHERE email = @playerEmail";

                var result = await connection.QueryFirstOrDefaultAsync<Player>(query, param: parameters);

                if (result is not null)
                    return 1;
            }
            catch(Exception ex) 
            {
                Debug.WriteLine(ex);
                return -1;

            }

            return 0;
        }
    }
}
