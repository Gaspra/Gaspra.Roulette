using System;
using System.Collections.Generic;

namespace Gaspra.Roulette.Api.Models
{
    public class Player
    {
        public Guid Identifier { get; }

        public string Name { get; }

        public string Secret { get; }

        public int TokenAllowance { get; set; }

        public int TokenSpikeAllowance { get; set; }

        public bool Active { get; }

        public IList<Token> Tokens;

        public Player(string name, string secret)
        {
            Identifier = Guid.NewGuid();

            Name = name;

            Secret = secret;

            TokenAllowance = 20;

            TokenSpikeAllowance = 0;

            Active = true;

            Tokens = new List<Token>();
        }

        public Player(Guid identifier, string name, string secret, int tokenAllowance, int tokenSpikeAllowance, bool active)
        {
            Identifier = identifier;

            Name = name;

            Secret = secret;

            TokenAllowance = tokenAllowance;

            TokenSpikeAllowance = tokenSpikeAllowance;

            Active = active;
        }
    }
}
