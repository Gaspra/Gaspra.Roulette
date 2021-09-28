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
        public async Task<string> GetPlayersNames([FromQuery]string delimiter = ", ")
        {
            var players = await _rouletteDataAccess.GetPlayers();

            var names = players
                .ToList()
                .Select(p => $"{p.Name}");

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
        public async Task<JsonResult> NewPlayer([FromQuery] string name, [FromQuery] string secret)
        {
            var players = await _rouletteDataAccess.GetPlayers();

            var playerName = name.Sanitise();

            if (string.IsNullOrWhiteSpace(playerName) || playerName.Length > 7)
            {
                return new JsonResult(new NewPlayerModel
                    {Reason = $"Name must be between 1-7 characters, sanitised name was: \"{playerName}\", please pick another!"})
                {
                    StatusCode = 406
                };
            }

            if (players.Any(p => p.Name.Equals(playerName, StringComparison.InvariantCultureIgnoreCase)))
            {
                return new JsonResult(new NewPlayerModel
                    {Reason = $"Player with name \"{playerName}\" already exists, try another name!"})
                {
                    StatusCode = 406
                };
            }

            if (string.IsNullOrWhiteSpace(secret))
            {
                return new JsonResult(new NewPlayerModel
                    {Reason = $"Please pick a secret, this will come in handy later!"})
                {
                    StatusCode = 406
                };
            }

            await _rouletteDataAccess.AddPlayer(new Player(playerName, secret));

            var allPlayers = await _rouletteDataAccess.GetPlayers();

            var playerModel = allPlayers.FirstOrDefault(p => p.Name.Equals(playerName));

            if (playerModel is null)
            {
                return new JsonResult(new NewPlayerModel
                    {Reason = $"Something went wrong, please try joining again!"})
                {
                    StatusCode = 418
                };
            }

            return new JsonResult(new NewPlayerModel
                {Reason = $"Enjoy playing {playerName}!"})
            {
                StatusCode = 201
            };
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

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("Spike")]
        public async Task<JsonResult> SpikePlayer([FromQuery] string attacker, [FromQuery] string secret, [FromQuery] string target)
        {
            if (attacker.Equals(target, StringComparison.InvariantCultureIgnoreCase))
            {
                return new JsonResult(new NewPlayerModel
                    {Reason = $"You can't spike yourself dummy..!"})
                {
                    StatusCode = 406
                };
            }

            var players = await _rouletteDataAccess.GetPlayers();

            var attackerPlayer =
                players.First(p => p.Name.Equals(attacker, StringComparison.InvariantCultureIgnoreCase));

            if (attackerPlayer.TokenSpikeAllowance <= 0)
            {
                return new JsonResult(new NewPlayerModel
                    {Reason = $"Try again when you have some spike tokens!"})
                {
                    StatusCode = 406
                };
            }

            if (!attackerPlayer.Secret.Equals(secret, StringComparison.InvariantCultureIgnoreCase))
            {
                return new JsonResult(new NewPlayerModel
                    {Reason = $"Incorrect secret code, try again!"})
                {
                    StatusCode = 406
                };
            }

            var targetPlayer =
                players.FirstOrDefault(p => p.Name.Equals(target, StringComparison.InvariantCultureIgnoreCase));

            if (targetPlayer is null)
            {
                return new JsonResult(new NewPlayerModel
                    {Reason = $"Unable to find the spike target \"{target}\", try again!"})
                {
                    StatusCode = 406
                };
            }

            targetPlayer.TokenAllowance += attackerPlayer.TokenSpikeAllowance;

            attackerPlayer.TokenSpikeAllowance = 0;

            var playerList = new List<Player>
            {
                targetPlayer,
                attackerPlayer
            };

            await _rouletteDataAccess.UpdatePlayers(playerList);

            return new JsonResult(new NewPlayerModel
                {Reason = $"You spiked {targetPlayer.Name}, they now have {targetPlayer.TokenAllowance} tokens!"})
            {
                StatusCode = 200
            };
        }
    }
}
