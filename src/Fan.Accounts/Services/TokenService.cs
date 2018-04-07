using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Fan.Accounts.Data;
using Fan.Accounts.Models;
using Fan.Exceptions;
using Fan.Helpers;
using Fan.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fan.Accounts.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepo;
        private readonly IOptionsSnapshot<BearerTokensOptions> _configuration;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public TokenService(ITokenRepository tokenRepo,
            IOptionsSnapshot<BearerTokensOptions> configuration,
            UserManager<User> userManager,
            RoleManager<Role> roleManager
            )
        {
            _configuration = configuration;
            _tokenRepo = tokenRepo;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// Creates and returns AccessToken and RefreshToken for user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<(string accessToken, string refreshToken)> CreateTokens(User user)
        {
            var now = DateTimeOffset.UtcNow;
            var accessTokenExpiresOn = now.AddMinutes(_configuration.Value.AccessTokenExpirationMinutes);
            var refreshTokenExpiresOn = now.AddMinutes(_configuration.Value.RefreshTokenExpirationMinutes);
            var accessToken = await CreateAccessTokenAsync(user, accessTokenExpiresOn.UtcDateTime).ConfigureAwait(false);
            var refreshToken = Guid.NewGuid().ToString().Replace("-", "");
            var accessTokenHash = Util.GetSha256Hash(accessToken);
            var refreshTokenHash = Util.GetSha256Hash(refreshToken);

            await DeleteTokensForUserAsync(user.Id);
            await _tokenRepo.CreateTokensAsync(user, accessTokenHash, refreshTokenHash, accessTokenExpiresOn, refreshTokenExpiresOn).ConfigureAwait(false);

            return (accessToken, refreshToken);
        }

        public async Task DeleteExpiredTokensAsync()
        {
            await _tokenRepo.DeleteExpiredTokensAsync();
        }

        public async Task DeleteTokensForUserAsync(int userId)
        {
            await _tokenRepo.DeleteTokensForUserAsync(userId);
        }

        public async Task<UserToken> FindRefreshTokenAsync(string refreshToken)
        {
            var hash = Util.GetSha256Hash(refreshToken);
            return await _tokenRepo.FindRefreshTokenAsync(hash);
        }

        public async Task ValidateTokenContextAsync(TokenValidatedContext context)
        {
            var userPrincipal = context.Principal;

            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;
            if (claimsIdentity?.Claims == null || !claimsIdentity.Claims.Any())
            {
                context.Fail("This is not our issued token. It has no claims.");
                return;
            }

            var serialNumberClaim = claimsIdentity.FindFirst(ClaimTypes.SerialNumber);
            if (serialNumberClaim == null)
            {
                context.Fail("This is not our issued token. It has no serial.");
                return;
            }

            var userIdString = claimsIdentity.FindFirst(ClaimTypes.UserData).Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                context.Fail("This is not our issued token. It has no user-id.");
                return;
            }

            var user = await _userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);
            if (user == null || user.SerialNumber != serialNumberClaim.Value/* || !user.IsActive*/)
            {
                // user has changed his/her password/roles/stat/IsActive
                context.Fail("This token is expired. Please login again.");
            }

            var accessToken = context.SecurityToken as JwtSecurityToken;
            if (accessToken == null || string.IsNullOrWhiteSpace(accessToken.RawData) ||
                !await IsAccessTokenValidAsync(accessToken.RawData, userId))
            {
                context.Fail("This token is not in our database.");
                return;
            }
        }

        private async Task<string> CreateAccessTokenAsync(User user, DateTime expires)
        {
            var claims = new List<Claim>
            {
                // Unique Id for all Jwt tokes
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // Issuer
                new Claim(JwtRegisteredClaimNames.Iss, _configuration.Value.Issuer),
                // Issued at
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("DisplayName", user.DisplayName),
                // to invalidate the cookie
                new Claim(ClaimTypes.SerialNumber, user.SerialNumber),
                // custom data
                new Claim(ClaimTypes.UserData, user.Id.ToString())
            };

            // add roles
            var roles = await _userManager.GetRolesAsync(user);
            //var roles = await _rolesService.FindUserRolesAsync(user.Id).ConfigureAwait(false);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration.Value.Issuer,
                audience: _configuration.Value.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<bool> IsAccessTokenValidAsync(string accessToken, int userId)
        {
            var hash = Util.GetSha256Hash(accessToken);
            var token = await _tokenRepo.FindAccessTokenAsync(hash, userId);
            return token?.ExpiresOn >= DateTime.UtcNow;
        }
    }
}
