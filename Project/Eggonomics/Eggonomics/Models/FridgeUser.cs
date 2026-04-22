namespace Eggonomics.Models
{
    public class FridgeUser
    {
        public int Id { get; set; }
        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }

        public FridgeUser() { }

        public FridgeUser(int id, Fridge fridge, User user)
        {
            Id = id;
            Fridge = fridge;
            FridgeId = fridge.Id;
            User = user;
            UserId = user.Id;
            JoinedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
