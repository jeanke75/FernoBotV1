namespace FernoBotV1.Services.Database.Models
{
    public enum ArmorType
    {
        Helmet = 'H',
        Upper = 'U',
        Pants = 'P',
        Boots = 'B',
        Gauntlets = 'G',
        Mantle = 'M',
        Shield = 'S'
    }

    public class Armor : Item
    {
        public ArmorType subtype { get; set; }
        public short defense { get; set; }
        public byte strength { get; set; }
        public byte dexterity { get; set; }
        public byte stamina { get; set; }
        public byte luck { get; set; }
    }
}
