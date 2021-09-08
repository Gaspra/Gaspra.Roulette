using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Interfaces;
using Gaspra.Roulette.Api.Models;

namespace Gaspra.Roulette.Api.Implementations
{
    public class PlayerService : IPlayerService
    {
        private readonly IRouletteDataAccess _rouletteDataAccess;

        public PlayerService(IRouletteDataAccess rouletteDataAccess)
        {
            _rouletteDataAccess = rouletteDataAccess;
        }

        public Task AddToken(Player player, Token token)
        {
            var tokenList = player.Tokens;

            tokenList ??= new List<Token>();

            tokenList.Add(token);

            player.Tokens = tokenList;

            return Task.CompletedTask;
        }

        public Task ResetPlayersTokens(IList<Player> players)
        {
            foreach (var player in players)
            {
                player.Tokens = new List<Token>();
            }

            return Task.CompletedTask;
        }

        public Task<Player> PickPlayer(IList<Player> players, Token token)
        {
            var pickedPlayer = players.First(p => p.Tokens.Any(t => t.Reference.Equals(token.Reference)));

            return Task.FromResult(pickedPlayer);
        }

        public async Task UpdatePlayersTokenAllowance(IList<Player> players, Player pickedPlayer)
        {
            foreach (var player in players.Where(p => !p.Equals(pickedPlayer)))
            {
                player.TokenAllowance += 1;
            }

            pickedPlayer.TokenAllowance -= (players.Count-1);

            if (pickedPlayer.TokenAllowance < 1)
            {
                pickedPlayer.TokenAllowance = 1;
            }

            await _rouletteDataAccess.UpdatePlayers(players);
        }
    }
}