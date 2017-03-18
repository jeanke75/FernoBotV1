using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord.Commands;
using FernoBotV1.Services.Database.Models;
using System.Collections.Generic;
using FernoBotV1.Services.Exceptions;

namespace FernoBotV1.Modules.Games.RPG
{
    public class EquipmentModule : ModuleBase
    {

        /*[Command("Equip")]
        [Summary("Equip an item from your inventory")]
        [Priority(1)]
        public async Task Equip(int itemId)
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
                            long userId = await AccountModule.GetUserIDAsync(conn, Context.Message.Author);
                            Item item = await ItemModule.GetSpecificItemInfoAsync(conn, itemId);
                            if (await InventoryModule.GetAmountOfSpecificItemInInventory(conn, userId, item) == 0)
                                throw new RPGException($"{Context.Message.Author.Username}, you don't have this item.");
                            await EquipItemAsync(conn, tr, userId, item);
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
            catch (RPGUserNotFoundException)
            {
                await ReplyAsync($"{Context.Message.Author.Username}, you haven't started your adventure yet. Type !create to begin.");
            }
            catch (RPGException ex)
            {
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Failed to start adventure for {Context.Message.Author.Username} {ex.ToString()}");
            }
        }

        [Command("Equip")]
        [Summary("Equip an item from your inventory")]
        [Priority(2)]
        public async Task Equip(string itemName)
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
                            // check if an item with that name is in the players inventory
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
            catch (Exception ex)
            {
                await ReplyAsync($"Failed to start adventure for {Context.Message.Author.Username} {ex.ToString()}");
            }
        }*/

        private async static Task EquipItemAsync(SqlConnection conn, SqlTransaction tr, long userId, Item item)
        {
            string colName = "";
            switch (item.type)
            {
                case ItemType.Weapon:
                    colName = "WeaponID";
                    break;
                case ItemType.Armor:

                    switch ((item as Armor).subtype)
                    {
                        case ArmorType.Helmet:
                            colName = "HelmetID";
                            break;
                        case ArmorType.Upper:
                            colName = "UpperID";
                            break;
                        case ArmorType.Pants:
                            colName = "PantsID";
                            break;
                        case ArmorType.Boots:
                            colName = "BootsID";
                            break;
                        case ArmorType.Gauntlets:
                            colName = "GloveID";
                            break;
                        case ArmorType.Mantle:
                            colName = "MantleID";
                            break;
                        case ArmorType.Shield:
                            colName = "ShieldID";
                            break;
                    }
                    break;                    
            }

            if (colName == "") throw new RPGInvalidItemTypeException($"{item.name} can't be equipped.");

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.AddWithValue("@user", userId);
                cmd.Parameters.AddWithValue("@item", item.id);
                cmd.CommandText = $"update Equipped set {colName} = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /*private async static Task EquipItemsAsync(SqlConnection conn, SqlTransaction tr, long userId, List<Item> item)
        {
            string colName = "";
            switch (item.type)
            {
                case ItemType.Weapon:
                    colName = "WeaponID";
                    break;
                case ItemType.Armor:

                    switch ((item as Armor).subtype)
                    {
                        case ArmorType.Helmet:
                            colName = "HelmetID";
                            break;
                        case ArmorType.Upper:
                            colName = "UpperID";
                            break;
                        case ArmorType.Pants:
                            colName = "PantsID";
                            break;
                        case ArmorType.Boots:
                            colName = "BootsID";
                            break;
                        case ArmorType.Gauntlets:
                            colName = "GloveID";
                            break;
                        case ArmorType.Mantle:
                            colName = "MantleID";
                            break;
                        case ArmorType.Shield:
                            colName = "ShieldID";
                            break;
                    }
                    break;
            }

            if (colName == "") throw new RPGInvalidItemTypeException($"{item.name} can't be equipped.");

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.AddWithValue("@user", userId);
                cmd.Parameters.AddWithValue("@item", item.id);
                cmd.CommandText = $"update Equipped set {colName} = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }


            string cmdString = "";
            foreach (var kvp in items)
            {
                cmd.Parameters.Add("@item" + kvp.Key, DbType.Int32).Value = kvp.Key;
                cmd.Parameters.Add("@amount" + kvp.Key, DbType.Int32).Value = kvp.Value;
                cmdString += $"insert into Inventory (UserID, ItemID, Amount) values (@user, @item{kvp.Key}, @amount{kvp.Value});";
            }
        }*/
    }
}
