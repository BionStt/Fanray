using Fan.Accounts.Models;
using Fan.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Accounts
{
    public interface ITokenStoreService
    {
        //Task AddUserTokenAsync(UserTokenOrig userToken);
        //Task AddUserTokenAsync(
        //        User user, string refreshToken, string accessToken,
        //        DateTimeOffset refreshTokenExpiresDateTime, DateTimeOffset accessTokenExpiresDateTime);
        Task<bool> IsValidTokenAsync(string accessToken, int userId);
        Task DeleteExpiredTokensAsync();
        Task<UserTokenOrig> FindTokenAsync(string refreshToken);
        //Task DeleteTokenAsync(string refreshToken);
        Task InvalidateUserTokensAsync(int userId);
        Task<(string accessToken, string refreshToken)> CreateJwtTokens(User user);
    }
}
