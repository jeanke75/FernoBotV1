using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using FernoBotV1.Services.Database.Models;

namespace FernoBotV1.Modules.Games.RPG
{
    public class RpgHelper
    {
        public static SqlConnection GetConnection()
        {
            Uri uri = new Uri(ConfigurationManager.AppSettings["SQLSERVER_URI"]);
            string connectionString = new SqlConnectionStringBuilder
            {
                DataSource = uri.Host,
                InitialCatalog = uri.AbsolutePath.Trim('/'),
                UserID = uri.UserInfo.Split(':').First(),
                Password = uri.UserInfo.Split(':').Last(),
            }.ConnectionString;

            return new SqlConnection(connectionString);
        }

        internal static Item ParseItem(SqlDataReader reader)
        {
            Item item;
            ItemType itemtype = (ItemType)((string)reader["Type"])[0];
            switch (itemtype)
            {
                case ItemType.Weapon:
                    item = new Weapon()
                    {
                        id = (int)reader["ItemID"],
                        name = (string)reader["Name"],
                        levelReq = (byte)reader["Level"],
                        valueBuy = (int)reader["ValueBuy"],
                        valueSell = (int)reader["ValueSell"],
                        attack_min = (short)reader["AttackMin"],
                        attack_max = (short)reader["AttackMax"],
                        critical = (byte)reader["Critical"],
                        strength = (byte)reader["Strength"],
                        dexterity = (byte)reader["Dexterity"],
                        stamina = (byte)reader["Stamina"],
                        luck = (byte)reader["Luck"]
                    };
                    break;
                case ItemType.Armor:
                    item = new Armor()
                    {
                        id = (int)reader["ItemID"],
                        name = (string)reader["Name"],
                        subtype = (ArmorType)((string)reader["SubType"])[0],
                        levelReq = (byte)reader["Level"],
                        valueBuy = (int)reader["ValueBuy"],
                        valueSell = (int)reader["ValueSell"],
                        defense = (short)reader["Defense"],
                        strength = (byte)reader["Strength"],
                        dexterity = (byte)reader["Dexterity"],
                        stamina = (byte)reader["Stamina"],
                        luck = (byte)reader["Luck"]
                    };
                    break;
                case ItemType.Potion:
                    item = new Potion()
                    {
                        id = (int)reader["ItemID"],
                        name = (string)reader["Name"],
                        levelReq = (byte)reader["Level"],
                        valueBuy = (int)reader["ValueBuy"],
                        heal = (short)reader["Heal"]
                    };
                    break;
                default:
                    item = new Item()
                    {
                        id = (int)reader["ItemID"],
                        name = (string)reader["Name"],
                        levelReq = (byte)reader["Level"],
                        valueBuy = (int)reader["ValueBuy"]
                    };
                    break;
            }
            return item;
        }
    }
}
