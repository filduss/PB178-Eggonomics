using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Eggonomics.Enums;

namespace Eggonomics.Models
{
    public partial class EggTray : ObservableObject
    {
        public int Id { get; set; }
        public int QuantityAdded { get; set; }

        [ObservableProperty]
        public partial int QuantityRemaining { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public Suppliers Origin { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public decimal Price { get; set; }
        public decimal PricePerEgg { get; set; }
        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; } = null!;
        public int AddedByUserId { get; set; }
        public User AddedBy { get; set; } = null!;
        public DateTime AddedAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public EggTray() { }

        public EggTray(int quantityAdded, string name, Suppliers origin, DateOnly expirationDate, decimal price, decimal pricePerEgg, Fridge fridge, User addedBy, string? notes = null)
        {
            QuantityAdded = quantityAdded;
            QuantityRemaining = quantityAdded;
            Name = name;
            Origin = origin;
            ExpirationDate = expirationDate;
            Price = price;
            PricePerEgg = pricePerEgg;
            Fridge = fridge;
            FridgeId = fridge.Id;
            AddedBy = addedBy;
            AddedByUserId = addedBy.Id;
            AddedAt = DateTime.UtcNow;
            Notes = notes;
        }
    }
}
