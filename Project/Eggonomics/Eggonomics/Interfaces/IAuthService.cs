using System;
using System.Collections.Generic;
using System.Text;

namespace Eggonomics.Interfaces
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task<bool> CreateAccountAsync(string email, string password);
        Task SignOutAsync();
    }
}
