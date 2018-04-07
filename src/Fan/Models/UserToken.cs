using Microsoft.AspNetCore.Identity;
using System;

namespace Fan.Models
{
    /// <summary>
    /// Represents a token provided by Fanray.
    /// </summary>
    /// <remarks>
    /// I follow the OAuth2 spec as such there are two tokens created for each user,
    /// AccessToken and RefreshToken <see cref="https://tools.ietf.org/html/rfc6749#section-1.4"/>
    /// </remarks>
    public class UserToken : IdentityUserToken<int>
    {
        public const string LOGIN_PROVIDER = "Fanray";

        public UserToken()
        {
            LoginProvider = LOGIN_PROVIDER;
        }

        public DateTimeOffset ExpiresOn { get; set; }
    }
}
