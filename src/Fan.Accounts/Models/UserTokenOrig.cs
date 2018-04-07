using Fan.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fan.Accounts.Models
{
    /// <summary>
    /// formerly UserToken
    /// </summary>
    public class UserTokenOrig
    {
        public int Id { get; set; }

        public string AccessTokenHash { get; set; }

        public DateTimeOffset AccessTokenExpiresDateTime { get; set; }

        public string RefreshTokenIdHash { get; set; }

        public DateTimeOffset RefreshTokenExpiresDateTime { get; set; }

        public int UserId { get; set; } // one-to-one association
        public virtual User User { get; set; }
    }
}
