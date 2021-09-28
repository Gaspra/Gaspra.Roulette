using System.Collections.Generic;

namespace Gaspra.Roulette.Api.Models
{
    public class AdminViewModel
    {
        public IList<Player> Players { get; }

        public int RollInterval { get; }

        public string SpikeTokenAllocation { get; }

        public AdminViewModel(IList<Player> players, int rollInterval, (int minWinner, int maxWinner, int minLoser, int maxLoser) spikeTokenAllocation)
        {
            Players = players;

            RollInterval = rollInterval;

            SpikeTokenAllocation =
                $"{spikeTokenAllocation.minWinner},{spikeTokenAllocation.maxWinner},{spikeTokenAllocation.minLoser},{spikeTokenAllocation.maxLoser}";
        }
    }
}
