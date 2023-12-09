using GameResults.Model;

namespace GameResults.DatabasAccess
{
    public interface IGameResultRepository
    {
        Task<int> AddUserResult(UserResultRequest userResultRequest);

        Task<List<TopResultsResponse>> GetTopResults(int count);
    }
}
