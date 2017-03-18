using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord.Commands;
using FernoBotV1.Services.Database.Models;
using FernoBotV1.Services.Exceptions;
using System.Collections.Concurrent;
using System.Linq;
using System;

namespace FernoBotV1.Modules.Games.RPG
{
    public class ItemModule : ModuleBase
    {
        /// <summary>
        /// Dictionaty that allows a lookup by name
        /// </summary>
        public static ConcurrentDictionary<string, Tuple<int, string>> ItemLookup { get; set; }

        public static bool ItemExists(int id) => ItemLookup.Values.Any(x => x.Item1 == id);

        public static async Task InitItemLookup(SqlConnection conn)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "select ItemID, Name, lower(Name) as LowerName from Items";
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    try
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();
                            ItemLookup.AddOrUpdate((string)reader["LowerName"], _ => Tuple.Create((int)reader["ItemID"], (string)reader["Name"]), (_, x) => Tuple.Create((int)reader["ItemID"], (string)reader["Name"]));
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }




        /*[Command("Item")]
        [Summary("Get info about an item using the id")]
        public async Task ItemInfo(int itemId)
        {
            try
            {
                using (SqlConnection conn = RpgHelper.GetConnection())
                {
                    await conn.OpenAsync();
                    using (SqlTransaction tr = conn.BeginTransaction())
                    {
                        try
                        {
                            Item item = await GetSpecificItemInfoAsync(conn, tr, itemId);
                            await ReplyAsync(item.name);
                        }
                        catch (Exception ex)
                        {
                            tr.Rollback();
                            throw ex;
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (RPGException ex)
            {
                await ReplyAsync($"{Context.Message.Author.Username},{ex.Message}");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Failed to start adventure for {Context.Message.Author.Username} {ex.ToString()}");
            }
        }*/
        /// <summary>
        /// Search all items by the given query
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tr"></param>
        /// <param name="searchQuery"></param>
        /// <returns></returns>
        public static async Task<List<Item>> SearchItemsAsync(SqlConnection conn, SqlTransaction tr, string searchQuery)
        {
            searchQuery = searchQuery.ToLowerInvariant();
            var collection = ItemLookup.Keys.Where(x => x.Contains(searchQuery)).OrderBy(str => str == searchQuery ? 0 : str.StartsWith(searchQuery) ? 1 : 2);
            var toReturn = new List<Item>();
            if (!collection.Any())
                return toReturn;
            if (collection.First() == searchQuery)
            {
                collection = collection.Take(1).OrderBy(x => 0);
            }
            foreach (var itemName in collection)
            {
                int itemId = ItemLookup[itemName].Item1;
                toReturn.Add(await GetSpecificItemInfoAsync(conn, tr, itemId));
            }
            return toReturn;
        }


        public static async Task<List<Item>> GetItemsInfoAsync(SqlConnection conn, SqlTransaction tr, List<int> itemIds)
        {
            List<Item> result = new List<Item>();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                List<string> views = new List<string>
                {
                    "WeaponsView",
                    "ArmorsView",
                    "PotionsView",
                    "ItemsView"
                };
                foreach (var itemId in itemIds)
                {
                    Item item = null;
                    if (!cmd.Parameters.Contains("@item"))
                    {
                        cmd.Parameters.Add(new SqlParameter("@item", DbType.Int32));
                    }
                    cmd.Parameters["@item"].Value = itemId;
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
                    if (item == null)
                    {
                        throw new RPGItemNotFoundException(itemId);
                    }
                    result.Add(item);
                }
                return result;
            }
        }

        public static async Task<Item> GetSpecificItemInfoAsync(SqlConnection conn, SqlTransaction tr, int itemId)
        {
            Item item = null;
            using (SqlCommand cmd = conn.CreateCommand())
            {
                List<string> views = new List<string>();
                views.Add("WeaponsView");
                views.Add("ArmorsView");
                views.Add("PotionsView");
                views.Add("ItemsView");

                cmd.Transaction = tr;
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
        [Obsolete]
        public static async Task<Item> GetSpecificItemInfoAsync(SqlConnection conn, SqlTransaction tr, string itemName)
        {
            Item item = null;
            using (SqlCommand cmd = conn.CreateCommand())
            {
                List<string> views = new List<string>();
                views.Add("WeaponsView");
                views.Add("ArmorsView");
                views.Add("PotionsView");
                views.Add("ItemsView");

                cmd.Transaction = tr;
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

        [Obsolete]
        public static async Task<List<Item>> GetSimilarItemInfoAsync(SqlConnection conn, SqlTransaction tr, string itemName)
        {
            List<Item> items = new List<Item>();
            using (SqlCommand cmd = conn.CreateCommand())
            {
                List<string> views = new List<string>();
                views.Add("WeaponsView");
                views.Add("ArmorsView");
                views.Add("PotionsView");
                views.Add("ItemsView");

                cmd.Transaction = tr;
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
