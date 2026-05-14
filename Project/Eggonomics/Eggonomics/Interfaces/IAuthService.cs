using System;
using System.Collections.Generic;
using System.Text;

namespace Eggonomics.Interfaces
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task<string?> CreateAccountAsync(string email, string password, string displayName);
        Task SignOutAsync();
    }
}
