using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord.Commands;
using FernoBotV1.Services.Database.Models;

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
            
            switch (item.type)
            {
                case ItemType.Weapon:
                    await EquipWeaponAsync(conn, tr, userId, item as Weapon);
                    break;
                case ItemType.Armor:
                    switch ((item as Armor).subtype)
                    {
                        case ArmorType.Helmet:
                            await EquipHelmetAsync(conn, tr, userId, item as Armor);
                            break;
                        case ArmorType.Upper:
                            await EquipUpperAsync(conn, tr, userId, item as Armor);
                            break;
                        case ArmorType.Pants:
                            await EquipPantsAsync(conn, tr, userId, item as Armor);
                            break;
                        case ArmorType.Boots:
                            await EquipBootsAsync(conn, tr, userId, item as Armor);
                            break;
                        case ArmorType.Gauntlets:
                            await EquipGauntletsAsync(conn, tr, userId, item as Armor);
                            break;
                        case ArmorType.Mantle:
                            await EquipMantleAsync(conn, tr, userId, item as Armor);
                            break;
                        case ArmorType.Shield:
                            await EquipShieldAsync(conn, tr, userId, item as Armor);
                            break;
                    }
                    break;
            }
        }

        private async static Task EquipWeaponAsync(SqlConnection conn, SqlTransaction tr, long userId, Weapon weapon)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@item", DbType.Int32).Value = weapon.id;
                cmd.CommandText = "update Equipped set WeaponID = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async static Task EquipHelmetAsync(SqlConnection conn, SqlTransaction tr, long userId, Armor helmet)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@item", DbType.Int32).Value = helmet.id;
                cmd.CommandText = "update Equipped set HelmetID = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async static Task EquipUpperAsync(SqlConnection conn, SqlTransaction tr, long userId, Armor upper)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@item", DbType.Int32).Value = upper.id;
                cmd.CommandText = "update Equipped set UpperID = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async static Task EquipPantsAsync(SqlConnection conn, SqlTransaction tr, long userId, Armor pants)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@item", DbType.Int32).Value = pants.id;
                cmd.CommandText = "update Equipped set PantsID = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async static Task EquipBootsAsync(SqlConnection conn, SqlTransaction tr, long userId, Armor boots)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@item", DbType.Int32).Value = boots.id;
                cmd.CommandText = "update Equipped set BootsID = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async static Task EquipGauntletsAsync(SqlConnection conn, SqlTransaction tr, long userId, Armor gauntlets)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@item", DbType.Int32).Value = gauntlets.id;
                cmd.CommandText = "update Equipped set GloveID = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async static Task EquipMantleAsync(SqlConnection conn, SqlTransaction tr, long userId, Armor mantle)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@item", DbType.Int32).Value = mantle.id;
                cmd.CommandText = "update Equipped set MantleID = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async static Task EquipShieldAsync(SqlConnection conn, SqlTransaction tr, long userId, Armor shield)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@item", DbType.Int32).Value = shield.id;
                cmd.CommandText = "update Equipped set ShieldID = @item where UserID = @user";
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
