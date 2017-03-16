using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord.Commands;
using FernoBotV1.Services.Database.Models;

namespace FernoBotV1.Modules.Games.RPG
{
    public class InventoryModule : ModuleBase
    {
        public async static Task AddItemToInventoryAsync(SqlConnection conn, SqlTransaction tr, long userId, long itemId, int amount)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId.ToString();
                cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;
                cmd.Parameters.Add("@amount", DbType.Int32).Value = amount;
                cmd.CommandText = $"insert into Inventory (UserID, ItemID, Amount) values (@user, @item, @amount)";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async static Task AddItemToInventoryAsync(SqlConnection conn, SqlTransaction tr, long userId, string itemName, int amount)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId.ToString();
                cmd.Parameters.Add("@item", DbType.String).Value = itemName;
                cmd.Parameters.Add("@amount", DbType.Int32).Value = amount;
                cmd.CommandText = $"insert into Inventory (UserID, ItemID, Amount) values (@user, (select ItemID from Items where Name = @item), @amount)";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async static Task<int> GetAmountOfSpecificItemInInventory(SqlConnection conn, long userId, Item item)
        {
            int amount = 0;
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@item", DbType.Int32).Value = item.id;
                cmd.CommandText = "select Amount from Inventory where UserID = @user and ItemID = @item";
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();
                        amount = (int)reader["Amount"];
                    }
                    reader.Close();
                }
            }
            return amount;
        }

        //public async static Task<List<Item>> AreItemsInInventory(List<Item> items)
    }
}
