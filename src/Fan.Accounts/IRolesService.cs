using Fan.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Accounts
{
    public interface IRolesService
    {
        Task<List<AdminRole>> FindUserRolesAsync(int userId);
        Task<bool> IsUserInRole(int userId, string roleName);
        Task<List<AdminUser>> FindUsersInRoleAsync(string roleName);
    }
}
