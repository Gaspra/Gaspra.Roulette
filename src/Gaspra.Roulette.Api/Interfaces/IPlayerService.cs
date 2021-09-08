using System.Collections.Generic;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Models;

namespace Gaspra.Roulette.Api.Interfaces
{
    public interface IPlayerService
    {
        Task AddToken(Player player, Token token);

        Task ResetPlayersTokens(IList<Player> players);

        Task<Player> PickPlayer(IList<Player> players, Token token);

        Task UpdatePlayersTokenAllowance(IList<Player> players, Player pickedPlayer);
    }
}