using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Gaspra.Roulette.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouletteController : Controller
    {
        private IList<IPlayer> _players { get; }

        public RouletteController()
        {
            _players = new List<IPlayer>
            {
                new Player("Richard", 10),
                new Player("Charley", 10),
                new Player("David", 10)
            };
        }

        [HttpGet]
        [Route("Roll")]
        public async Task<string> Roll()
        {
            var playerTokens = new List<Token>();

            foreach (var player in _players)
            {
                await player.ResetTokens();
            }

            var tokenPool = _players.Select(p => p.MaxTokens).Sum();

            for (var t = 0; t < tokenPool; t++)
            {
                playerTokens.Add(new Token(t));
            }

            playerTokens = playerTokens.OrderBy(t => Guid.NewGuid()).ToList();

            int assignedCount = 0;

            while (assignedCount < playerTokens.Count)
            {
                foreach (var player in _players)
                {
                    if ((await player.GetTokens()).Count < player.MaxTokens)
                    {
                        await player.AddToken(playerTokens[assignedCount]);

                        assignedCount++;
                    }
                }
            }

            var random = new Random(Guid.NewGuid().GetHashCode());

            var randomToken = new Token(random.Next(0, playerTokens.Count+1));

            IPlayer randomPlayer = null;

            foreach (var player in _players)
            {
                if (await player.ContainsToken(randomToken))
                {
                    randomPlayer = player;

                    await player.UpdateMaxToken(player.MaxTokens - 5);
                }
                else
                {
                    await player.UpdateMaxToken(player.MaxTokens + 5);
                }
            }

            var playerInformation = "";

            foreach (var player in _players)
            {
                playerInformation += $"{player.Name}: ";

                foreach (var token in await player.GetTokens())
                {
                    playerInformation += $"{token.Reference}, ";
                }

                playerInformation += $"{Environment.NewLine}{Environment.NewLine}";
            }

            return $"player: {randomPlayer.Name}, with token: {randomToken.Reference}. {Environment.NewLine}{Environment.NewLine}{playerInformation}";
        }
    }

    public class Token
    {
        public int Reference { get; }

        public Token(int reference)
        {
            Reference = reference;
        }
    }

    public interface IPlayer
    {
        string Name { get; }

        Task AddToken(Token token);

        Task ResetTokens();

        Task<IList<Token>> GetTokens();

        int MaxTokens { get; set; }

        Task<bool> ContainsToken(Token token);

        Task UpdateMaxToken(int value);
    }

    public class Player : IPlayer
    {
        public string Name { get; }

        public int MaxTokens { get; set; }

        private IList<Token> _tokens;

        public Player(string name, int seedTokens)
        {
            Name = name;
            _tokens = new List<Token>();

            var random = new Random(Guid.NewGuid().GetHashCode());

            var randomTokenMax = random.Next(6, 20);

            MaxTokens = randomTokenMax;
        }

        public Task AddToken(Token token)
        {
            _tokens ??= new List<Token>();

            _tokens.Add(token);

            return Task.CompletedTask;
        }

        public Task ResetTokens()
        {
            _tokens = new List<Token>();

            return Task.CompletedTask;
        }

        public Task<IList<Token>> GetTokens()
        {
            return Task.FromResult(_tokens);
        }

        public Task<bool> ContainsToken(Token token)
        {
            return Task.FromResult(_tokens.Any(t => t.Reference.Equals(token.Reference)));
        }

        public Task UpdateMaxToken(int value)
        {
            MaxTokens = value;

            return Task.CompletedTask;
        }
    }
}
