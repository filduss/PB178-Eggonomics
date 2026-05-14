using System.ComponentModel.DataAnnotations;

namespace Eggonomics.Models
{
    public class Fridge
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public List<FridgeUser> Members { get; set; } = [];
        public List<EggTray> EggTrays { get; set; } = [];

        public Fridge() { }

        public Fridge(int id, string name, User owner)
        {
            Id = id;
            Name = name;
            Owner = owner;
            OwnerId = owner.Id;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
