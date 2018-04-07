using Fan.Accounts.Models;
using Fan.Data;
using Fan.Models;
using System;
using System.Threading.Tasks;

namespace Fan.Accounts.Data
{
    public interface ITokenRepository : IRepository<UserToken>
    {
        //Task AddUserTokenAsync(UserToken userToken);
        /// <summary>
        /// Creates a AccessToken and RefreshToken for user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="accessTokenHash"></param>
        /// <param name="refreshTokenHash"></param>
        /// <param name="accessTokenExpiresOn"></param>
        /// <param name="refreshTokenExpiresOn"></param>
        /// <returns></returns>
        Task CreateTokensAsync(
                User user, 
                string accessTokenHash,
                string refreshTokenHash,
                DateTimeOffset accessTokenExpireOn,
                DateTimeOffset refreshTokenExpireOn);

        /// <summary>
        /// Deletes all expired tokens.
        /// </summary>
        /// <returns></returns>
        Task DeleteExpiredTokensAsync();

        Task<UserToken> FindAccessTokenAsync(string accessTokenHash, int userId);
        Task<UserToken> FindRefreshTokenAsync(string refreshTokenHash);

        //Task DeleteTokenAsync(string refreshToken);
        /// <summary>
        /// Deletes the AccessToken and RefreshToken for user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task DeleteTokensForUserAsync(int userId);
    }
}
