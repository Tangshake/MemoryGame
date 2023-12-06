
using Dapper;
using LoginService.AuthTokens.Model;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;

namespace LoginService.DatabaseAccess
{
    public class TokenRepository(IConfiguration configuration) : ITokenRepository
    {
        private readonly IConfiguration configuration = configuration;
        private readonly string ConnectionStringName = "MemoryGame";

        public async Task<int> CreateRefreshTokenAsync(int userId, RefreshToken refreshToken)
        {
            try
            {
                var connectionString = configuration.GetConnectionString(ConnectionStringName);
                await using NpgsqlConnection connection = new (connectionString);

                var parameters = new { player_id = userId, token = refreshToken.Token, created = refreshToken.Created, expire = refreshToken.Expired };

                var query = @"INSERT INTO reftoken (player_id, token, created, expire)
                                VALUES (@player_id, @token, @created, @expire) 
                                ON CONFLICT (player_id) 
                                DO UPDATE SET token = EXCLUDED.token, created = EXCLUDED.created, expire = EXCLUDED.expire";
                ;

                var result = await connection.ExecuteAsync(query, param: parameters);

                return result;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e);
                return -1;
            }
        }

        public async Task<string> GetJwtSecKeyAsync()
        {
            try
            {
                var connectionString = configuration.GetConnectionString(ConnectionStringName);
                await using NpgsqlConnection connection = new(connectionString);

                var query = " SELECT * FROM jwt";

                string? jwt_key = await connection.ExecuteScalarAsync<string>(query);

                return jwt_key!;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

            }

            return null;
        }

        public Task<string> GetRefreshTokenAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
