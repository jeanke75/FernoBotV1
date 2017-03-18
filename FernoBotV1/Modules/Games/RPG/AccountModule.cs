using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using FernoBotV1.Preconditions;
using FernoBotV1.Services.Database.Models;
using FernoBotV1.Services.Exceptions;

namespace FernoBotV1.Modules.Games.RPG
{
    public class AccountModule : ModuleBase
    {
        public static Dictionary<Item, int> DefaultItemCollection { get; set; } = new Dictionary<Item, int>
        {
            {
                new Weapon()
                {
                    name = "wooden sword",
                    type = ItemType.Weapon
                },
                1
            },
            {
                new Armor()
                {
                    name = "cloth headband",
                    type = ItemType.Armor,
                    subtype = ArmorType.Helmet
                },
                1
            },
            {
                new Armor()
                {
                    name = "cloth shirt",
                    type = ItemType.Armor,
                    subtype = ArmorType.Upper
                },
                1
            },
            {
                new Armor()
                {
                    name = "cloth pants",
                    type = ItemType.Armor,
                    subtype = ArmorType.Pants
                },
                1
            },
            {
                new Armor()
                {
                    name = "cloth boots",
                    type = ItemType.Armor,
                    subtype = ArmorType.Boots
                },
                1
            },
            {
                new Armor()
                {
                    name = "cloth gloves",
                    type = ItemType.Armor,
                    subtype = ArmorType.Gauntlets
                },
                1
            }
        };

        [Command(nameof(Create))]
        [Summary("Start your adventure")]
        [Cooldown(0, 1, 0)]
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
                            if (await GetUserIDAsync(conn, tr, Context.Message.Author) == 0)
                            {
                                long userId = await CreateUserAsync(conn, tr, Context.Message.Author);
                                if (DefaultItemCollection.First().Key.id == default(int))
                                {
                                    foreach (var kvp in DefaultItemCollection)
                                    {
                                        kvp.Key.id = ItemModule.ItemLookup[kvp.Key.name].Item1;
                                    }
                                }
                                var d = DefaultItemCollection.ToDictionary(s => s.Key.id, s => s.Value);

                                await InventoryModule.AddItemsToInventoryAsync(conn, tr, userId, d);

                                await ReplyAsync($"{Context.Message.Author.Username}, your adventure has started. May the Divine spirits guide you on your adventures. (!help for a list of commands)");
                            }
                            else
                            {
                                await ReplyAsync($"{Context.Message.Author.Username}, you've already started your adventure.");
                            }

                        }
                        catch (Exception ex)
                        {
                            await ReplyAsync(ex.StackTrace.Substring(0, 1500));

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

        /*
                //create cooldowns
                cmd.Parameters.Add("@DateTime", DbType.DateTime).Value = new DateTime(2000, 1, 1, 0, 0, 0);
                cmd.CommandText = "insert into Cooldowns(UserID, Start, Stats, Assign, Inventory, Equip, Donate, Info, Shop, Attack, Heal, Craft) " +
                                    "values (@user, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime, @DateTime)";
                await cmd.ExecuteNonQueryAsync();*/
    }
}
