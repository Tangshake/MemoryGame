using MemoryGame.Model;

namespace MemoryGame.SynchronousDataService.Highscore;

public interface IHighscoreService
{
    Task<bool> AddAsync(GameResultModelRequest gameResultModelRequest, string requestUri);

    Task<List<TopGamesResultsModelResponse>> GetAsync(int count, int userId, string requestUri);
}
