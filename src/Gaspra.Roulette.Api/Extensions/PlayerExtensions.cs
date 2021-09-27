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
        public static async Task<IList<Player>> PlayersFromDataReader(this SqlDataReader sqlDataReader)
        {
            var players = new List<Player>();

            while (await sqlDataReader.ReadAsync())
            {
                var identifier = sqlDataReader["Identifier"].GetValue<Guid>();

                var name = sqlDataReader["Name"].GetValue<string>();

                var secret = sqlDataReader["Secret"].GetValue<string>();

                var tokenAllowance = sqlDataReader["TokenAllowance"].GetValue<int>();

                var tokenSpikeAllowance = sqlDataReader["TokenSpikeAllowance"].GetValue<int>();

                var active = sqlDataReader["Active"].GetValue<int>();

                players.Add(new Player(identifier, name, secret, tokenAllowance, tokenSpikeAllowance, Convert.ToBoolean(active)));
            }

            return players;
        }

        public static DataTable PlayersToDataTable(this IList<Player> players)
        {
            var properties = TypeDescriptor.GetProperties(typeof(Player));

            var playerDataTable = new DataTable();

            foreach (PropertyDescriptor prop in properties)
            {
                if (!prop.Name.Equals("tokens", StringComparison.InvariantCultureIgnoreCase))
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
