using Fan.Data;
using Fan.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Accounts.Data
{
    /// <summary>
    /// Operations on the UserToken table.
    /// </summary>
    public class SqlTokenRepository : EntityRepository<UserToken>, ITokenRepository
    {
        const string ACCESS_TOKEN = "AccessToken";
        const string REFRESH_TOKEN = "RefreshToken";

        private readonly FanDbContext _db;
        public SqlTokenRepository(FanDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task CreateTokensAsync(User user, string accessTokenHash, string refreshTokenHash,
            DateTimeOffset accessTokenExpiresOn, DateTimeOffset refreshTokenExpiresOn)
        {
            var tokenAccess = new UserToken
            {
                UserId = user.Id,
                Name = ACCESS_TOKEN,
                Value = accessTokenHash,
                ExpiresOn = accessTokenExpiresOn
            };

            var tokenRefresh = new UserToken
            {
                UserId = user.Id,
                Name = REFRESH_TOKEN,
                Value = refreshTokenHash,
                ExpiresOn = refreshTokenExpiresOn
            };

            await _db.AddAsync(tokenAccess);
            await _db.AddAsync(tokenRefresh);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all expired tokens.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteExpiredTokensAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var tokens = _entities.Where(x => x.ExpiresOn < now && x.LoginProvider == UserToken.LOGIN_PROVIDER);
            _entities.RemoveRange(tokens);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteTokensForUserAsync(int userId)
        {
            var tokens = _entities.Where(x => x.UserId == userId && x.LoginProvider == UserToken.LOGIN_PROVIDER);
            _entities.RemoveRange(tokens);
            await _db.SaveChangesAsync();
        }

        public async Task<UserToken> FindAccessTokenAsync(string accessTokenHash, int userId) =>
            await _entities.FirstOrDefaultAsync(
                x => x.Name == ACCESS_TOKEN
                && x.Value == accessTokenHash
                && x.LoginProvider == UserToken.LOGIN_PROVIDER
                && x.UserId == userId);


        public async Task<UserToken> FindRefreshTokenAsync(string refreshTokenHash) =>
            await _entities.FirstOrDefaultAsync(x => x.Name == REFRESH_TOKEN
                                                  && x.LoginProvider == UserToken.LOGIN_PROVIDER
                                                  && x.Value == refreshTokenHash);

        //public async Task<UserToken> FindRefreshTokenAsync(string refreshTokenHash)
        //{
        //    //var userRolesQuery = from user in _db.Set<User>()
        //    //                     from userTokens in user.tok
        //    //                     where userRoles.UserId == userId
        //    //                     select role;

        //    return userRolesQuery.OrderBy(x => x.Name).ToListAsync();
        //}

    }
}
