namespace FernoBotV1.Services.Database.Models
{
    public enum ItemType
    {
        Item = 'I',
        Weapon = 'W',
        Armor = 'A',
        Potion = 'P'
    }

    public class Item
    {
        public int id { get; set; }
        public string name { get; set; }
        public ItemType type { get; set; }
        public byte levelReq { get; set; }
        public int valueBuy { get; set; }
        public int valueSell { get; set; }
    }
}
