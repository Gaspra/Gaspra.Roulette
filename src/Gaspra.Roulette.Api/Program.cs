using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Controllers;
using Gaspra.Roulette.Api.Implementations;
using Gaspra.Roulette.Api.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gaspra.Roulette.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IPlayerService, PlayerService>();
                    services.AddSingleton<IRouletteDataAccess, RouletteDataAccess>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
