using FernoBotV1.Services.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FernoBotV1.Extensions
{
    public static class ItemExtensions
    {
        public static bool IsEquippable(this Item it) => it is Armor || it is Weapon;

    }
}
