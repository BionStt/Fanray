using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Fan.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    /// <remarks>
    /// It's derived from <see cref=" IdentityUser"/> with added properties.
    /// </remarks>
    public class User : IdentityUser<int>
    {
        public User()
        {
            CreatedOn = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// When the user was created.
        /// </summary>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// The friendly name to display on posts.
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        public string DisplayName { get; set; }

        /// <summary>
        /// A GUID <see cref="GenerateSerialNumber"/> used for validate user token context 
        /// Fan.Accounts.TokenSerivce.ValidateTokenContextAsync(). 
        /// A new one is generated every time a user changes password, role or status etc.
        /// </summary>
        [StringLength(maximumLength: 256)]
        public string SerialNumber { get; set; }

        public static string GenerateSerialNumber()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
