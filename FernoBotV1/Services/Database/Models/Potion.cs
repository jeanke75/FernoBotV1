namespace FernoBotV1.Services.Database.Models
{
    public class Potion : Item
    {
        public Potion()
        {
            type = ItemType.Potion;
        }

        public short heal { get; set; }
    }
}
