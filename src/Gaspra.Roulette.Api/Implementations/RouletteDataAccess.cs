using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Extensions;
using Gaspra.Roulette.Api.Interfaces;
using Gaspra.Roulette.Api.Models;
using Microsoft.Extensions.Configuration;

namespace Gaspra.Roulette.Api.Implementations
{
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

                command.Parameters.Add("@Prefix", SqlDbType.NVarChar).Value = player.Prefix;

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

                command.Parameters.Add("@Identifier", SqlDbType.UniqueIdentifier).Value = player.Identifier;

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task AddHistory(Guid identifier, DateTimeOffset rollTimestamp)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[AddHistory]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@Identifier", SqlDbType.UniqueIdentifier).Value = identifier;

                command.Parameters.Add("@RollTimestamp", SqlDbType.DateTimeOffset).Value = rollTimestamp;

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<(int rollCount, int historyCount)> GetHistoryForPlayer(Guid identifier)
        {
            (int rollCount, int historyCount) value = (-1, -1);

            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[GetHistoryForPlayer]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@Identifier", SqlDbType.UniqueIdentifier).Value = identifier;

                await connection.OpenAsync();

                await using var dataReader = await command.ExecuteReaderAsync();

                while (await dataReader.ReadAsync())
                {
                    var rollCount = dataReader["RollCount"].GetValue<int>();

                    var historyCount = dataReader["HistoryCount"].GetValue<int>();

                    value = (rollCount, historyCount);
                }

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }

            return value;
        }

        public async Task<IList<HistoricalRoll>> GetHistory()
        {
            var historicalRolls = new List<HistoricalRoll>();

            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[GetHistory]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await connection.OpenAsync();

                await using var dataReader = await command.ExecuteReaderAsync();

                while (await dataReader.ReadAsync())
                {
                    var prefix = dataReader["Prefix"].GetValue<string>();

                    var name = dataReader["Name"].GetValue<string>();

                    var rollTimestamp = dataReader["RollTimestamp"].GetValue<DateTimeOffset>();

                    historicalRolls.Add(new HistoricalRoll(prefix, name, rollTimestamp.LocalDateTime));
                }

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }

            return historicalRolls;
        }

        public async Task DeletePlayer(Guid identifier)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[DeletePLayer]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@Identifier", SqlDbType.UniqueIdentifier).Value = identifier;

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<int> GetRollInterval()
        {
            var rollInterval = 60 * 10;

            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[GetRollInterval]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await connection.OpenAsync();

                await using var dataReader = await command.ExecuteReaderAsync();

                while (await dataReader.ReadAsync())
                {
                    rollInterval = dataReader["RollInterval"].GetValue<int>();
                }

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }

            return rollInterval;
        }

        public async Task UpdateRollInterval(int rollInterval)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[UpdateRollInterval]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@RollInterval", SqlDbType.Int).Value = rollInterval;

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task UpdatePlayerTokens(Guid identifier, int tokens)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[UpdatePlayerTokens]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("@Identifier", SqlDbType.UniqueIdentifier).Value = identifier;

                command.Parameters.Add("@Tokens", SqlDbType.Int).Value = tokens;

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task ResetEverything()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("[Roulette].[ResetEverything]", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

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
