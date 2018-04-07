using Fan.Accounts.Models;
using Fan.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Accounts.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Creates and returns AccessToken and RefreshToken for user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<(string accessToken, string refreshToken)> CreateTokens(User user);

        Task DeleteExpiredTokensAsync();
        Task DeleteTokensForUserAsync(int userId);

        Task<UserToken> FindRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Validates token context and indicates that there was a failure during authentication.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task ValidateTokenContextAsync(TokenValidatedContext context);
    }
}
