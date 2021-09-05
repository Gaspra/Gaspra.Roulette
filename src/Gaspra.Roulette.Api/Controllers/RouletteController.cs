using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Gaspra.Roulette.Api.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class RouletteController : Controller
    {
        private readonly IPlayerService _playerService;
        private readonly IRouletteDataAccess _rouletteDataAccess;

        public RouletteController(
            IPlayerService playerService,
            IRouletteDataAccess rouletteDataAccess)
        {
            _playerService = playerService;
            _rouletteDataAccess = rouletteDataAccess;
        }

        [HttpGet]
        [Route("TempAddPlayers")]
        public async Task AddPlayers()
        {
            await _rouletteDataAccess.AddPlayer(new Player("Richard"));

            await _rouletteDataAccess.AddPlayer(new Player("Charley"));

            await _rouletteDataAccess.AddPlayer(new Player("David"));
        }

        [HttpGet]
        [Route("Roll")]
        public async Task<string> Roll()
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

            var playerInformation = "";

            foreach (var player in players)
            {
                playerInformation += $"{player.Name} - allowance [{player.TokenAllowance}]: ";

                foreach (var token in player.Tokens)
                {
                    playerInformation += $"{token.Reference}";

                    if (!player.Tokens.Last().Equals(token))
                    {
                        playerInformation += ", ";
                    }
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

    public interface IPlayerService
    {
        Task AddToken(Player player, Token token);

        Task ResetPlayersTokens(IList<Player> players);

        Task<Player> PickPlayer(IList<Player> players, Token token);

        Task UpdatePlayersTokenAllowance(IList<Player> players, Player pickedPlayer);
    }

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

    public class Player
    {
        public Guid Identifier { get; }

        public string Name { get; }

        public int TokenAllowance { get; set; }

        public bool Active { get; }

        public IList<Token> Tokens;

        public Player(string name)
        {
            Identifier = Guid.NewGuid();

            Name = name;

            TokenAllowance = 20;

            Active = true;

            Tokens = new List<Token>();
        }

        public Player(Guid identifier, string name, int tokenAllowance, bool active)
        {
            Identifier = identifier;

            Name = name;

            TokenAllowance = tokenAllowance;

            Active = active;
        }
    }

    public static class PlayerExtensions
    {
        public static async Task<IList<Player>> PlayersFromDataReader(this SqlDataReader sqlDataReader)
        {
            var players = new List<Player>();

            while (await sqlDataReader.ReadAsync())
            {
                var identifier = sqlDataReader["Identifier"].GetValue<Guid>();

                var name = sqlDataReader["Name"].GetValue<string>();

                var tokenAllowance = sqlDataReader["TokenAllowance"].GetValue<int>();

                var active = sqlDataReader["Active"].GetValue<int>();

                players.Add(new Player(identifier, name, tokenAllowance, Convert.ToBoolean(active)));
            }

            return players;
        }

        public static DataTable PlayersToDataTable(this IList<Player> players)
        {
            var properties = TypeDescriptor.GetProperties(typeof(Player));

            var playerDataTable = new DataTable();

            foreach (PropertyDescriptor prop in properties)
            {
                if (!prop.Name.Contains("tokens", StringComparison.InvariantCultureIgnoreCase))
                {
                    playerDataTable
                        .Columns
                        .Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
            }

            foreach (var player in players)
            {
                var row = playerDataTable.NewRow();

                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(player) ?? DBNull.Value;
                }

                playerDataTable.Rows.Add(row);
            }

            return playerDataTable;
        }
    }

    public static class DataReaderExtensions
    {
        public static T GetValue<T>(this object data)
        {
            if (data.GetType().Equals(typeof(DBNull)))
            {
                return default;
            }

            var nullableType = Nullable.GetUnderlyingType(typeof(T));

            if (nullableType != null)
            {
                return (T)Convert.ChangeType(data, nullableType);
            }

            return (T)data;
        }

        public static DateTime? GetDateTimeOrNull(this object data)
        {
            if (data.GetType().Equals(typeof(DBNull)))
            {
                return null;
            }

            var dateValue = (DateTime)data;

            if (DateTime.MinValue.Equals(dateValue))
            {
                return null;
            }

            return dateValue;
        }
    }

    public interface IRouletteDataAccess
    {
        Task AddPlayer(Player player);

        Task<IList<Player>> GetPlayers();

        Task UpdatePlayers(IList<Player> players);

        Task TogglePlayer(Player player);
    }

    public class RouletteDataAccess : IRouletteDataAccess
    {
        private readonly string _connectionString;

        public RouletteDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString"];
        }

        public async Task AddPlayer(Player player)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[AddPlayer]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@Identifier", SqlDbType.UniqueIdentifier).Value = player.Identifier;

                command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = player.Name;

                command.Parameters.Add("@TokenAllowance", SqlDbType.Int).Value = player.TokenAllowance;

                command.Parameters.Add("@Active", SqlDbType.Int).Value = 1;

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<IList<Player>> GetPlayers()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[GetPlayers]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await connection.OpenAsync();

                await using var dataReader = await command.ExecuteReaderAsync();

                var players = await dataReader.PlayersFromDataReader();

                await connection.CloseAsync();

                return players;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task UpdatePlayers(IList<Player> players)
        {
            await using var connection = new SqlConnection(_connectionString);

            var command = new SqlCommand("[Roulette].[UpdatePlayers]", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            var playerDataTable = players.PlayersToDataTable();

            var playerTableParameter = command.Parameters.AddWithValue("@Players", playerDataTable);

            playerTableParameter.SqlDbType = SqlDbType.Structured;

            playerTableParameter.TypeName = "Roulette.TT_UpdatePlayers";

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }

        public async Task TogglePlayer(Player player)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[TogglePlayer]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("Identifier", SqlDbType.UniqueIdentifier).Value = player.Identifier;

                command.Parameters.Add("Active", SqlDbType.NVarChar).Value = player.Name;

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
