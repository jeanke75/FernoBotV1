namespace FernoBotV1.Services.Database.Models
{
    public class Weapon : Item
    {
        public Weapon()
        {
            type = ItemType.Weapon;
        }
        public short attack_min { get; set; }
        public short attack_max { get; set; }
        public byte critical { get; set; }
        public byte strength { get; set; }
        public byte dexterity { get; set; }
        public byte stamina { get; set; }
        public byte luck { get; set; }
    }
}
