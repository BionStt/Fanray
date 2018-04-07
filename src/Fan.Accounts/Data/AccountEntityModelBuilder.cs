using Fan.Accounts.Models;
using Fan.Data;
using Fan.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fan.Accounts.Data
{
    public class AccountEntityModelBuilder : IEntityModelBuilder
    {
        public void CreateModel(ModelBuilder builder)
        {
            builder.Entity<User>().ToTable("Core_User");
            builder.Entity<Role>().ToTable("Core_Role");
            builder.Entity<IdentityUserClaim<int>>().ToTable("Core_UserClaim");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("Core_RoleClaim");
            builder.Entity<IdentityUserRole<int>>().ToTable("Core_UserRole");
            builder.Entity<IdentityUserLogin<int>>().ToTable("Core_UserLogin");
            builder.Entity<UserToken>().ToTable("Core_UserToken");
        }
    }
}
