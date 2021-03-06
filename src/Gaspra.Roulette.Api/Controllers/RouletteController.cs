using System.Linq;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Interfaces;
using Gaspra.Roulette.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gaspra.Roulette.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class RouletteController : Controller
    {
        private readonly IRouletteDataAccess _rouletteDataAccess;

        public RouletteController(
            IRouletteDataAccess rouletteDataAccess)
        {
            _rouletteDataAccess = rouletteDataAccess;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var players = await _rouletteDataAccess.GetPlayers();

            var history = (await _rouletteDataAccess.GetHistory())
                .ToList()
                .OrderByDescending(h => h.RollTimestamp)
                .Take(4)
                .ToList();

            return View(new RouletteViewModel(players, history));
        }
    }
}
