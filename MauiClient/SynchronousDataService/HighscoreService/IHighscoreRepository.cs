using MemoryGame.Model;

namespace MemoryGame.SynchronousDataService.HighscoreService;

public interface IHighscoreRepository
{
    Task<bool> AddAsync(GameResultModelRequest gameResultModelRequest, string requestUri);

    Task<List<TopGamesResultsModelResponse>> GetAsync(int count, string requestUri);
}
