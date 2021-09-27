using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Extensions;
using Gaspra.Roulette.Api.Interfaces;
using Gaspra.Roulette.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gaspra.Roulette.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayersController : Controller
    {
        private readonly IRouletteDataAccess _rouletteDataAccess;

        public PlayersController(
            IRouletteDataAccess rouletteDataAccess)
        {
            _rouletteDataAccess = rouletteDataAccess;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<Player>> GetPlayers()
        {
            var players = await _rouletteDataAccess.GetPlayers();

            return players.ToList();
        }

        [HttpGet]
        [Route("Names")]
        public async Task<string> GetPlayersNames([FromQuery]bool includePrefix = true, [FromQuery]string delimiter = ", ")
        {
            var players = await _rouletteDataAccess.GetPlayers();

            var names = players
                .ToList()
                .Select(p => includePrefix ? $"{p.Prefix} {p.Name}" : $"{p.Name}");

            return string
                .Join(delimiter, names);
        }

        [HttpGet]
        [Route("{identifier}")]
        public async Task<Player> GetPlayer(Guid identifier)
        {
            var players = await _rouletteDataAccess.GetPlayers();

            var player = players.FirstOrDefault(p => p.Identifier.Equals(identifier));

            return player;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("New")]
        public async Task NewPlayer([FromQuery] string name)
        {
            var players = await _rouletteDataAccess.GetPlayers();

            var playerName = name.Sanitise();

            if (!string.IsNullOrWhiteSpace(playerName))
            {
                var playerPrefix = players.PickUniquePrefix(playerName);

                await _rouletteDataAccess.AddPlayer(new Player(playerName, playerPrefix));
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("Toggle")]
        public async Task TogglePlayer([FromQuery] Guid identifier)
        {
            var players = await _rouletteDataAccess.GetPlayers();

            var player = players.FirstOrDefault(p => p.Identifier.Equals(identifier));

            if (player is not null)
            {
                await _rouletteDataAccess.TogglePlayer(player);
            }
        }

        [HttpGet]
        [Route("{identifier}/History")]
        public async Task<string> GetHistoryForPlayer(Guid identifier)
        {
            var historyValues = await _rouletteDataAccess.GetHistoryForPlayer(identifier);

            return $"{historyValues.rollCount}/ {historyValues.historyCount}";
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("{identifier}/Delete")]
        public async Task DeletePlayer(Guid identifier)
        {
            await _rouletteDataAccess.DeletePlayer(identifier);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("{identifier}/Tokens")]
        public async Task UpdatePlayerTokens(Guid identifier, [FromQuery] int tokens)
        {
            await _rouletteDataAccess.UpdatePlayerTokens(identifier, tokens);
        }
    }
}
