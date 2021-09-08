using System;
using System.Collections.Generic;

namespace Gaspra.Roulette.Api.Models
{
    public class Player
    {
        public Guid Identifier { get; }

        public string Prefix { get; }

        public string Name { get; }

        public int TokenAllowance { get; set; }

        public bool Active { get; }

        public IList<Token> Tokens;

        public Player(string name, string prefix)
        {
            Identifier = Guid.NewGuid();

            Name = name;

            Prefix = prefix;

            TokenAllowance = 20;

            Active = true;

            Tokens = new List<Token>();
        }

        public Player(Guid identifier, string name, string prefix, int tokenAllowance, bool active)
        {
            Identifier = identifier;

            Name = name;

            Prefix = prefix;

            TokenAllowance = tokenAllowance;

            Active = active;
        }
    }
}