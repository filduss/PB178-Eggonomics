using System.ComponentModel.DataAnnotations;

namespace Eggonomics.Models
{
    public class DebtSettlement
    {
        public int Id { get; set; }
        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; } = null!;
        public int PayerUserId { get; set; }
        public User Payer { get; set; } = null!;
        public int ReceiverUserId { get; set; }
        public User Receiver { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime SettledAt { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }

        public DebtSettlement() { }

        public DebtSettlement(Fridge fridge, User payer, User receiver, decimal amount, string? notes = null)
        {
            Fridge = fridge;
            FridgeId = fridge.Id;
            Payer = payer;
            PayerUserId = payer.Id;
            Receiver = receiver;
            ReceiverUserId = receiver.Id;
            Amount = amount;
            SettledAt = DateTime.UtcNow;
            Notes = notes;
        }
    }
}
