using System.Diagnostics;
using Eggonomics.Data;
using Eggonomics.Interfaces;
using Eggonomics.Models;
using Microsoft.EntityFrameworkCore;

namespace Eggonomics.Services
{
    public class AuthService : IAuthService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly AppDbContext _db;

        public AuthService(Supabase.Client supabaseClient, AppDbContext db)
        {
            _supabaseClient = supabaseClient;
            _db = db;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var session = await _supabaseClient.Auth.SignIn(email, password);
                if (session == null) return false;

                await EnsureUserExistsAsync(email);
                return true;
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex)
            {
                Debug.WriteLine($"Supabase Auth Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateAccountAsync(string email, string password)
        {
            try
            {
                await _supabaseClient.Auth.SignUp(email, password);
                await EnsureUserExistsAsync(email);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Registration failed: {e.Message}");
                return false;
            }
        }

        public async Task SignOutAsync()
        {
            try
            {
                await _supabaseClient.Auth.SignOut();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sign out error: {ex.Message}");
            }
        }

        private async Task EnsureUserExistsAsync(string email)
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            if (!exists)
            {
                var username = email.Split('@')[0];
                _db.Users.Add(new User
                {
                    Email = email,
                    Username = username,
                    DisplayName = username,
                    PasswordHash = string.Empty,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }
        }
    }
}
