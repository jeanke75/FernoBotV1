using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord.Commands;
using FernoBotV1.Services.Database.Models;
using FernoBotV1.Services.Exceptions;

namespace FernoBotV1.Modules.Games.RPG
{
    public class ItemModule : ModuleBase
    {
        public static async Task<Item> GetSpecificItemInfoAsync(SqlConnection conn, int itemId)
        {
            Item item = null;
            using (SqlCommand cmd = conn.CreateCommand())
            {
                List<string> views = new List<string>();
                views.Add("WeaponsView");
                views.Add("ArmorsView");
                views.Add("PotionsView");
                views.Add("ItemsView");

                cmd.Parameters.Add("@item", DbType.Int32).Value = itemId;

                // do an exact search in each type of item
                foreach (string view in views)
                {
                    cmd.CommandText = string.Format("select * from {0} where ItemID = @item", view);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();
                            item = RpgHelper.ParseItem(reader);
                        }
                        reader.Close();
                    }
                    if (item != null) break;
                }
            }
            if (item == null) throw new RPGItemNotFoundException(itemId);
            return item;
        }

        public static async Task<Item> GetSpecificItemInfoAsync(SqlConnection conn, string itemName)
        {
            Item item = null;
            using (SqlCommand cmd = conn.CreateCommand())
            {
                List<string> views = new List<string>();
                views.Add("WeaponsView");
                views.Add("ArmorsView");
                views.Add("PotionsView");
                views.Add("ItemsView");

                cmd.Parameters.Add("@name", DbType.String).Value = itemName;

                // do an exact search in each type of item
                foreach (string view in views)
                {
                    cmd.CommandText = string.Format("select * from {0} where lower(Name) = lower(@name)", view);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();
                            item = RpgHelper.ParseItem(reader);
                        }
                        reader.Close();
                    }
                    if (item != null) break;
                }
            }
            if (item == null) throw new RPGItemNotFoundException(itemName);
            return item;
        }

        public static async Task<List<Item>> GetSimilarItemInfoAsync(SqlConnection conn, string itemName)
        {
            List<Item> items = new List<Item>();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                List<string> views = new List<string>();
                views.Add("WeaponsView");
                views.Add("ArmorsView");
                views.Add("PotionsView");
                views.Add("ItemsView");

                cmd.Parameters.Add("@name", DbType.String).Value = itemName;

                // if no exact match is found look through all the views to find anything that contains the search
                if (items.Count == 0)
                {
                    foreach (string view in views)
                    {
                        cmd.CommandText = string.Format("select * from {0} where lower(Name) like lower('%' || @name || '%')", view);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(RpgHelper.ParseItem(reader));
                            }
                            reader.Close();
                        }
                    }
                }
            }
            if (items.Count == 0) throw new RPGItemNotFoundException(itemName);
            return items;
        }
    }
}
