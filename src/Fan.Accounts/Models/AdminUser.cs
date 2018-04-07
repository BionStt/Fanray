using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fan.Accounts.Models
{
    public class AdminUser
    {
        public AdminUser()
        {
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 256)]
        public string UserName { get; set; }

        public string Password { get; set; }

        public string DisplayName { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset? LastLoggedIn { get; set; }

        /// <summary>
        /// every time the user changes his Password,
        /// or an admin changes his Roles or stat/IsActive,
        /// create a new `SerialNumber` GUID and store it in the DB.
        /// </summary>
        public string SerialNumber { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }

        public virtual UserTokenOrig UserToken { get; set; }
    }

}
