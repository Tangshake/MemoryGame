using GameResults.Model;

namespace GameResults.DatabasAccess
{
    public interface IGameResultRepository
    {
        Task<int> AddUserResultAsync(UserResultRequest userResultRequest);

        Task<List<TopResultsResponse>> GetTopResultsAsync(int count);
    }
}
