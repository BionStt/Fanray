using System;
using System.Collections.Generic;
using System.Text;

namespace Fan.Accounts.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public virtual AdminUser User { get; set; }
        public virtual AdminRole Role { get; set; }
    }
}
