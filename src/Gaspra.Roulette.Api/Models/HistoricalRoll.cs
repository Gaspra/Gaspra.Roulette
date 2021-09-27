using System;

namespace Gaspra.Roulette.Api.Models
{
    public class HistoricalRoll
    {
        public string Name { get; }
        public DateTime RollTimestamp { get; }

        public HistoricalRoll(string name, DateTime rollTimestamp)
        {
            Name = name;

            RollTimestamp = rollTimestamp;
        }
    }
}
