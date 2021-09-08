using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Gaspra.Roulette.Api.Models;

namespace Gaspra.Roulette.Api.Extensions
{
    public static class PlayerExtensions
    {
        public static string PickUniquePrefix(this IList<Player> players, string name)
        {
            var prefixList = new List<string>
            {
                "Admiral", "The Honourable", "Captain",
                "Doctor", "President", "General",
                "Chancellor", "Holiness", "Chief",
                "Reverend", "Professor", "Count"
            };

            var duplicatePlayerPrefixes = players
                .Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                .Select(p => p.Prefix)
                .ToList();

            var uniquePrefixList = prefixList
                .Where(p => !duplicatePlayerPrefixes.Any(d =>
                    d.Equals(p, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            if (!uniquePrefixList.Any())
            {
                throw new Exception($"No unique prefixes for player with name [{name}] left");
            }

            var random = new Random(Guid.NewGuid().GetHashCode());

            var randomPrefix = uniquePrefixList[random.Next(0, uniquePrefixList.Count)];

            return randomPrefix;
        }

        public static async Task<IList<Player>> PlayersFromDataReader(this SqlDataReader sqlDataReader)
        {
            var players = new List<Player>();

            while (await sqlDataReader.ReadAsync())
            {
                var identifier = sqlDataReader["Identifier"].GetValue<Guid>();

                var name = sqlDataReader["Name"].GetValue<string>();

                var prefix = sqlDataReader["Prefix"].GetValue<string>();

                var tokenAllowance = sqlDataReader["TokenAllowance"].GetValue<int>();

                var active = sqlDataReader["Active"].GetValue<int>();

                players.Add(new Player(identifier, name, prefix, tokenAllowance, Convert.ToBoolean(active)));
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
}