namespace Eggonomics.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public User() { }

        public User(string username, string email, string passwordHash, string displayName, int id)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            DisplayName = displayName;
            Id = id;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
