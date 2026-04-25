using System;
using System.Collections.Generic;
using System.Text;
using Eggonomics.Interfaces;


namespace Eggonomics.Services
{
    public class AuthService : IAuthService
    {
        private readonly Supabase.Client _supabaseClient;

        public AuthService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var session = await _supabaseClient.Auth.SignIn(email, password);
                return session != null;
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Supabase Auth Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error Reason: {ex.Reason}");

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");
                return false;
            }

        }
    }
}
