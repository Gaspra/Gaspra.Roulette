using System;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Interfaces;
using Gaspra.Roulette.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gaspra.Roulette.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly IRouletteDataAccess _rouletteDataAccess;

        public AdminController(
            IRouletteDataAccess rouletteDataAccess)
        {
            _rouletteDataAccess = rouletteDataAccess;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index([FromQuery]string secret = "not the secret")
        {
            if (secret.Equals("eggyroy", StringComparison.InvariantCultureIgnoreCase))
            {
                var players = await _rouletteDataAccess.GetPlayers();

                var rollInterval = await _rouletteDataAccess.GetRollInterval();

                var spikeTokenAllocation = await _rouletteDataAccess.GetSpikeAllocation();

                return View(new AdminViewModel(players, rollInterval, spikeTokenAllocation));
            }

            return Redirect("~/");
        }

        [HttpPost]
        [Route("Reset")]
        public async Task ResetEverything()
        {
            await _rouletteDataAccess.ResetEverything();
        }

        [HttpPost]
        [Route("SpikeAllocation")]
        public async Task SpikeAllocation([FromQuery]int minWinner, [FromQuery]int maxWinner, [FromQuery]int minLoser, [FromQuery]int maxLoser)
        {
            await _rouletteDataAccess.SpikeAllocation(minWinner, maxWinner, minLoser, maxLoser);
        }

    }
}
