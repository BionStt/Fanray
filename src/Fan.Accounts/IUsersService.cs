using Fan.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Accounts
{
    public interface IUsersService
    {
        Task<string> GetSerialNumberAsync(int userId);
        Task<AdminUser> FindUserAsync(string username, string password);
        Task<AdminUser> FindUserAsync(int userId);
        Task UpdateUserLastActivityDateAsync(int userId);
    }
}
