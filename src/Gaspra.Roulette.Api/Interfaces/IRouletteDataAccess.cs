using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Models;

namespace Gaspra.Roulette.Api.Interfaces
{
    public interface IRouletteDataAccess
    {
        Task AddPlayer(Player player);

        Task<IList<Player>> GetPlayers();

        Task UpdatePlayers(IList<Player> players);

        Task TogglePlayer(Player player);

        Task AddHistory(Guid identifier, DateTimeOffset rollTimestamp);

        Task<(int rollCount, int historyCount)> GetHistoryForPlayer(Guid identifier);

        Task<IList<HistoricalRoll>> GetHistory();

        Task DeletePlayer(Guid identifier);

        Task<int> GetRollInterval();

        Task UpdateRollInterval(int rollInterval);

        Task UpdatePlayerTokens(Guid identifier, int tokens);

        Task ResetEverything();

        Task SpikeAllocation(int minWinner, int maxWinner, int minLoser, int maxLoser);

        Task<(int minWinner, int maxWinner, int minLoser, int maxLoser)> GetSpikeAllocation();
    }
}
