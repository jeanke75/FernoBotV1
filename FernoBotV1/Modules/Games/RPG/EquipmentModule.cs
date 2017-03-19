using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord.Commands;
using FernoBotV1.Services.Database.Models;
using System.Collections.Generic;
using FernoBotV1.Services.Exceptions;
using FernoBotV1.Extensions;

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

        public async static Task EquipItemAsync(SqlConnection conn, SqlTransaction tr, long userId, Item item)
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

        public async static Task EquipItemsAsync(SqlConnection conn, SqlTransaction tr, long userId, IEnumerable<Item> items)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.AddWithValue("@user", userId);

                string cmdString = "";
                foreach (Item item in items)
                {
                    if (!item.IsEquippable()) continue;

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
                    if (colName == "") continue;

                    cmd.Parameters.AddWithValue("@item" + item.id, item.id);
                    cmdString += $"update Equipped set {colName} = @item{item.id} where UserID = @user;";
                }

                cmd.CommandText = cmdString;
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
