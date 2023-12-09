using Dapper;
using GameResults.Model;
using Npgsql;

namespace GameResults.DatabasAccess
{
    public class GameResultRepository(IConfiguration configuration) : IGameResultRepository
    {
        private readonly IConfiguration configuration = configuration;
        private readonly string ConnectionStringName = "MemoryGame";

        public async Task<int> AddUserResult(UserResultRequest userResultRequest)
        {
            try
            {
                var connectionString = configuration.GetConnectionString(ConnectionStringName);
                var connection = new NpgsqlConnection(connectionString);

                var parameters = new { id = userResultRequest.Id, duration = userResultRequest.Duration, moves = userResultRequest.Moves, time = userResultRequest.Time };
                var request = "INSERT INTO score VALUES (@id, @duration, @moves, @time)";

                var result = await connection.ExecuteAsync(request, parameters);

                return result;
            }
            catch(Exception e)
            {
                return 0;
            }
        }

        public async Task<List<TopResultsResponse>> GetTopResults(int count)
        {
            try
            {
                var connectionString = configuration.GetConnectionString(ConnectionStringName);
                var connection = new NpgsqlConnection(connectionString);

                var parameters = new { count = count};
                var request = @"SELECT name, duration, moves, time FROM score
                                JOIN player 
                                ON score.player_id = id
                                ORDER BY result ASC LIMIT @count;
                              ";

                var result = await connection.QueryAsync<TopResultsResponse>(request, parameters);

                return result.ToList();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
