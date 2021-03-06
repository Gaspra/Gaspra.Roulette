using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Interfaces;
using Gaspra.Roulette.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gaspra.Roulette.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RollController : Controller
    {
        private readonly IPlayerService _playerService;
        private readonly IRouletteDataAccess _rouletteDataAccess;

        public RollController(
            IPlayerService playerService,
            IRouletteDataAccess rouletteDataAccess)
        {
            _playerService = playerService;
            _rouletteDataAccess = rouletteDataAccess;
        }

        [HttpGet]
        [Route("")]
        public async Task<string> Roll([FromQuery] int rollList = 0, [FromQuery] string delimiter = ",")
        {
            var playerTokens = new List<Token>();

            var players = (await _rouletteDataAccess
                .GetPlayers())
                .Where(p => p.Active)
                .ToList();

            await _playerService
                .ResetPlayersTokens(players);

            var tokenPool = players
                .Select(p => p.TokenAllowance)
                .Sum();

            for (var t = 0; t < tokenPool; t++)
            {
                playerTokens
                    .Add(new Token(t));
            }

            playerTokens = playerTokens
                .OrderBy(t => Guid.NewGuid())
                .ToList();

            var assignedCount = 0;

            while (assignedCount < playerTokens.Count)
            {
                foreach (var player in players)
                {
                    if (player.Tokens.Count < player.TokenAllowance)
                    {
                        await _playerService.AddToken(player, playerTokens[assignedCount]);

                        assignedCount++;
                    }
                }
            }

            var random = new Random(Guid.NewGuid().GetHashCode());

            var randomToken = new Token(random.Next(0, playerTokens.Count+1));

            var randomPlayer = await _playerService.PickPlayer(players, randomToken);

            await _playerService.UpdatePlayersTokenAllowance(players, randomPlayer);

            await _rouletteDataAccess.AddHistory(randomPlayer.Identifier, DateTimeOffset.UtcNow);

            var rollName = $"{randomPlayer.Name}";

            if (rollList > 0)
            {
                var randomPlayerList = new List<Player>();

                var randomPlayerNumber = -1;

                var nextRandomPlayerNumber = -1;

                for (var p = 0; p < rollList; p++)
                {
                    while (randomPlayerNumber.Equals(nextRandomPlayerNumber))
                    {
                        nextRandomPlayerNumber = random.Next(0, players.Count);
                    }

                    randomPlayerList.Add(players[nextRandomPlayerNumber]);

                    randomPlayerNumber = nextRandomPlayerNumber;
                }

                rollName = string.Join(delimiter,
                    randomPlayerList.Select(p => $"{p.Name}")) + delimiter + rollName;
            }

            return rollName;
        }

        [HttpGet]
        [Route("Interval")]
        public async Task<int> GetRollInterval()
        {
            var rollInterval = await _rouletteDataAccess.GetRollInterval();

            return rollInterval;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("Interval")]
        public async Task UpdateRollInterval([FromQuery]int rollInterval)
        {
            await _rouletteDataAccess.UpdateRollInterval(rollInterval);
        }
    }
}
