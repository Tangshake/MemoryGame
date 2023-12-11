using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Services;

public interface IGameResultRepository
{
    Task<bool> AddGameResultAsync(GameResultModelRequest gameResultModelRequest, string requestUri);

    Task<List<TopGamesResultsModelResponse>> GetTopResults(int count, string requestUri);
}
