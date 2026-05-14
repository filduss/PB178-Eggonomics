using System.ComponentModel.DataAnnotations;
using Eggonomics.Enums;

namespace Eggonomics.Models
{
    public class EggOperation
    {
        public int Id { get; set; }
        public OperationType Type { get; set; }
        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; } = null!;
        public int FridgeUserId { get; set; }
        public FridgeUser FridgeUser { get; set; } = null!;
        public int TotalQuantity { get; set; }
        public decimal MonetaryValue { get; set; }
        public DateTime PerformedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public int? EditedByUserId { get; set; }
        public User? EditedBy { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }

        public string TypeLabel => Type switch
        {
            OperationType.Insert => "gave",
            OperationType.Withdraw => "took",
            OperationType.Expire => "expired",
            _ => "unknown",
        };

        public List<EggOperationBatchLink> BatchLinks { get; set; } = [];

        public EggOperation() { }

        public EggOperation(OperationType type, Fridge fridge, FridgeUser fridgeUser, int totalQuantity, decimal monetaryValue = 0, string? notes = null)
        {
            Type = type;
            Fridge = fridge;
            FridgeId = fridge.Id;
            FridgeUser = fridgeUser;
            FridgeUserId = fridgeUser.Id;
            TotalQuantity = totalQuantity;
            MonetaryValue = monetaryValue;
            PerformedAt = DateTime.UtcNow;
            Notes = notes;
        }
    }
}
