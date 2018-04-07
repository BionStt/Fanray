using System;
using System.Collections.Generic;
using System.Text;

namespace Fan.Accounts.Models
{
    public class AdminRole
    {
        public AdminRole()
        {
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
