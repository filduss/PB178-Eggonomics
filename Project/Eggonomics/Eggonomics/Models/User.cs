using System.ComponentModel.DataAnnotations;

namespace Eggonomics.Models
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(254)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(100)]
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
