using System.Threading.Tasks;
using Gaspra.Roulette.Api.Interfaces;
using Gaspra.Roulette.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gaspra.Roulette.Api.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            var players = await _rouletteDataAccess.GetPlayers();

            var rollInterval = await _rouletteDataAccess.GetRollInterval();

            return View(new AdminViewModel(players, rollInterval));
        }

        [HttpPost]
        [Route("Reset")]
        public async Task ResetEverything()
        {
            await _rouletteDataAccess.ResetEverything();
        }
    }
}
