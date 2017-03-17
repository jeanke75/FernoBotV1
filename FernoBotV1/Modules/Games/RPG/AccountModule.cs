using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FernoBotV1.Services.Database.Models;
using FernoBotV1.Services.Exceptions;
using FernoBotV1.Preconditions;

namespace FernoBotV1.Modules.Games.RPG
{
    public class AccountModule : ModuleBase
    {
        [Command(nameof(Create))]
        [Summary("Start your adventure")]
        [Cooldown(0,1,0)]
        public async Task Create()
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
                            try
                            {
                                await GetUserIDAsync(conn, tr, Context.Message.Author);
                                await ReplyAsync($"{Context.Message.Author.Username}, you've already started your adventure.");
                            }
                            catch (RPGUserNotFoundException)
                            {
                                long userId = await CreateUserAsync(conn, tr, Context.Message.Author);
                                Task[] tasks = new Task[6];
                                tasks[0] = InventoryModule.AddItemToInventoryAsync(conn, tr, userId, weapon.id, 1);
                                tasks[1] = InventoryModule.AddItemToInventoryAsync(conn, tr, userId, "Cloth Headband", 1);
                                tasks[2] = InventoryModule.AddItemToInventoryAsync(conn, tr, userId, "Cloth Shirt", 1);
                                tasks[3] = InventoryModule.AddItemToInventoryAsync(conn, tr, userId, "Cloth Pants", 1);
                                tasks[4] = InventoryModule.AddItemToInventoryAsync(conn, tr, userId, "Cloth Boots", 1);
                                tasks[5] = InventoryModule.AddItemToInventoryAsync(conn, tr, userId, "Cloth Gloves", 1);

                                await Task.WhenAll(tasks).ContinueWith(_ => CreateEquippedItemsAsync(conn, tr, userId, "Wooden Sword", null, "Cloth Headband", "Cloth Shirt", "Cloth Pants", "Cloth Boots", "Cloth Gloves", null)).Unwrap();
                                await CreateEquippedItemsAsync(conn, tr, userId, "Wooden Sword", null, "Cloth Headband", "Cloth Shirt", "Cloth Pants", "Cloth Boots", "Cloth Gloves", null);
                                
                                await ReplyAsync($"{Context.Message.Author.Username}, your adventure has started. May the Divine spirits guide you on your adventures. (!help for a list of commands)");
                            }
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
        }

        public async static Task<long> GetUserIDAsync(SqlConnection conn, SqlTransaction tr, IUser discordUser)
        {
            long userId = 0;
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@DiscordID", DbType.String).Value = discordUser.Id.ToString();
                cmd.CommandText = "select UserID from Users where DiscordID = @DiscordID";
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();
                        userId = (long)reader["UserID"];
                    }
                    reader.Close();
                }
            }
            if (userId == 0) throw new RPGUserNotFoundException();
            return userId;
        }

        private async static Task<long> CreateUserAsync(SqlConnection conn, SqlTransaction tr, IUser discordUser)
        {
            // create user
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@discord", DbType.String).Value = discordUser.Id.ToString();
                cmd.CommandText = "insert into Users(DiscordID) values (@discord)";
                await cmd.ExecuteNonQueryAsync();
            }

            long userId = await GetUserIDAsync(conn, tr, discordUser);

            //TODO get initial hp from the hp calculation function so that any changes to the function are reflected on new accounts
            // create stats
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.CommandText = "insert into Stats (UserID, Health, Level, Experience, Strength, Dexterity, Stamina, Luck, Gold) " +
                                  "values (@user, 72, 1, 0, 0, 0, 0, 0, 0)";
                await cmd.ExecuteNonQueryAsync();
            }

            // create equipment
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.CommandText = "insert into Equipped (UserID, HelmetID, UpperID, PantsID, BootsID, GloveID, MantleID, ShieldID, WeaponID) " +
                                  "values (@user, null, null, null, null, null, null, null, null)";
                await cmd.ExecuteNonQueryAsync();
            }

            return userId;
        }

        private async static Task CreateEquippedItemsAsync(SqlConnection conn, SqlTransaction tr, long userId, string weaponName, string shieldName,
                                                           string helmetName, string upperName, string pantsName, string bootsName, string gauntletsName,
                                                           string mantleName)
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.Transaction = tr;
                cmd.Parameters.Add("@user", DbType.Int64).Value = userId;
                cmd.Parameters.Add("@helmet", DbType.String).Value = helmetName;
                cmd.Parameters.Add("@upper", DbType.String).Value = upperName;
                cmd.Parameters.Add("@pants", DbType.String).Value = pantsName;
                cmd.Parameters.Add("@boots", DbType.String).Value = bootsName;
                cmd.Parameters.Add("@gauntlets", DbType.String).Value = gauntletsName;
                cmd.Parameters.Add("@mantle", DbType.String).Value = mantleName;
                cmd.Parameters.Add("@shield", DbType.String).Value = shieldName;
                cmd.Parameters.Add("@weapon", DbType.String).Value = weaponName;
                cmd.CommandText = "insert into Equipped (UserID, HelmetID, UpperID, PantsID, BootsID, GloveID, MantleID, ShieldID, WeaponID) " +
                                  "values (@user, (select ItemID from Items where Name = @helmet), (select ItemID from Items where Name = @upper, " +
                                  "(select ItemID from Items where Name = @pants), (select ItemID from Items where Name = @boots), " +
                                  "(select ItemID from Items where Name = @gauntlets), (select ItemID from Items where Name = @mantle), " +
                                  "(select ItemID from Items where Name = @shield), (select ItemID from Items where Name = @weapon))";
                await cmd.ExecuteNonQueryAsync();
            }
        }
        /*
                //create cooldowns
                cmd.Parameters.Add("@DateTime", DbType.DateTime).Value = new DateTime(2000, 1, 1, 0, 0, 0);
                cmd.CommandText = "insert into Cooldowns(UserID, Start, Stats, Assign, Inventory, Equip, Donate, Info, Shop, Attack, Heal, Craft) " +
                                    "values (@user, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime)";
                await cmd.ExecuteNonQueryAsync();*/
    }
}
