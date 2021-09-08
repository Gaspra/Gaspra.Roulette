using System.Collections.Generic;

namespace Gaspra.Roulette.Api.Models
{
    public class RouletteViewModel
    {
        public IList<Player> Players { get; }

        public IList<HistoricalRoll> HistoricalRolls { get; }

        public RouletteViewModel(IList<Player> players, IList<HistoricalRoll> historicalRolls)
        {
            Players = players;

            HistoricalRolls = historicalRolls;
        }
    }
}
