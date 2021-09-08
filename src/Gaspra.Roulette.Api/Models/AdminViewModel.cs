using System.Collections.Generic;

namespace Gaspra.Roulette.Api.Models
{
    public class AdminViewModel
    {
        public IList<Player> Players { get; }

        public int RollInterval { get; }

        public AdminViewModel(IList<Player> players, int rollInterval)
        {
            Players = players;

            RollInterval = rollInterval;
        }
    }
}
