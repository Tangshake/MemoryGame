
using Dapper;
using Npgsql;
using RegisterService.Entity;
using System.Diagnostics;

namespace RegisterService.DatabaseAccess
{
    public class RegisterRepository(IConfiguration configuration) : IRegisterRepository
    {
        private readonly IConfiguration configuration = configuration;
        

        public async Task<Activation> GetUserActivationInformationAsync(int userId)
        {
            try
            {
                var connectionString = configuration.GetConnectionString("MemoryGame");
                var connection = new NpgsqlConnection(connectionString);

                var parameters = new { id = userId};

                // Upsert
                var query = "SELECT * FROM activation WHERE player_id = @id";

                var result = await connection.QueryFirstOrDefaultAsync<Activation>(query, param: parameters);

                return result!;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null!;
            }
        }

        public async Task<int> RemoveActivationEntryAsync(int userId)
        {
            try
            {
                var connectionString = configuration.GetConnectionString("MemoryGame");
                var connection = new NpgsqlConnection(connectionString);

                var parameters = new { id = userId };

                // Upsert
                var query = "DELETE FROM activation WHERE player_id = @id";

                var result = await connection.ExecuteAsync(query, param: parameters);

                return result!;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return -1;
            }
        }

        public async Task<int> SetUserActivationAsync(int userId, int tokenLifetime, string token)
        {
            try
            {
                var connectionString = configuration.GetConnectionString("MemoryGame");
                var connection = new NpgsqlConnection(connectionString);

                var parameters = new { id = userId, start = DateTime.Now, end = DateTime.Now.AddMinutes(tokenLifetime), secret = token };

                var query = "INSERT INTO activation VALUES (@id, @start, @end, @secret ) ON CONFLICT (player_id) DO UPDATE SET activation_start = @start, activation_end = @end";

                var result = await connection.ExecuteAsync(query, param: parameters);

                return result;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
                return -1;
            }
        }
    }
}
