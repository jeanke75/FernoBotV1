﻿using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord.Commands;
using FernoBotV1.Services.Database.Models;

namespace FernoBotV1.Modules.Games.RPG
{
    public class InventoryModule : ModuleBase
    {
        public async static Task AddItemToInventoryAsync(SqlConnection conn, SqlTransaction tr, long userId, int itemId, int amount)
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

        public async static Task AddItemsToInventoryAsync(SqlConnection conn, SqlTransaction tr, long userId, Dictionary<int, int> items)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId.ToString();

                string cmdString = "";
                foreach(var kvp in items)
                {
                    cmd.Parameters.Add("@item" + kvp.Key, DbType.Int32).Value = kvp.Key;
                    cmd.Parameters.Add("@amount" + kvp.Key, DbType.Int32).Value = kvp.Value;
                    cmdString += $"insert into Inventory (UserID, ItemID, Amount) values (@user, @item{kvp.Key}, @amount{kvp.Key});";
                }
                cmd.CommandText = cmdString;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async static Task<int> GetAmountOfSpecificItemInInventory(SqlConnection conn, SqlTransaction tr, long userId, Item item)
        {
            int amount = 0;
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
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
