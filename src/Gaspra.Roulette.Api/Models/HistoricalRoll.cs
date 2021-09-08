using System;

namespace Gaspra.Roulette.Api.Models
{
    public class HistoricalRoll
    {
        public string Prefix { get; }
        public string Name { get; }
        public DateTime RollTimestamp { get; }

        public HistoricalRoll(string prefix, string name, DateTime rollTimestamp)
        {
            Prefix = prefix;

            Name = name;

            RollTimestamp = rollTimestamp;
        }
    }
}
