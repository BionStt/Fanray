using Fan.Accounts.Api.Models;
using Fan.Accounts.Services;
using Fan.Exceptions;
using Fan.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Fan.Accounts.Api
{
    /// <summary>
    /// The api controller for admin console.
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("CorsPolicy")]
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;

        public AdminController(
            UserManager<User> userManager,
            SignInManager<User> signInManager, 
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginIM loginUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("User login failed.");
            }

            var user = await _userManager.FindByNameAsync(loginUser.UserName);
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginUser.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var (accessToken, refreshToken) = await _tokenService.CreateTokens(user);
            return Ok(new { access_token = accessToken, refresh_token = refreshToken });
        }

        /// <summary>
        /// Returns new AccessToken and RefreshToken.
        /// </summary>
        /// <param name="jsonBody"></param>
        /// <returns></returns>
        /// <remarks>
        /// TODO need better understand of oauth on what this action should do.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshToken([FromBody]JToken jsonBody)
        {
            var refreshToken = jsonBody.Value<string>("refreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest("refreshToken is not set.");
            }

            var token = await _tokenService.FindRefreshTokenAsync(refreshToken);
            if (token == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(token.UserId.ToString());
            var (accessTokenNew, refreshTokenNew) = await _tokenService.CreateTokens(user);
            return Ok(new { access_token = accessTokenNew, refresh_token = refreshTokenNew });
        }

        [AllowAnonymous]
        [HttpGet("[action]"), HttpPost("[action]")]
        public async Task<bool> Logout()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            var userIdValue = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;

            // The Jwt implementation does not support "revoke OAuth token" (logout) by design.
            // Delete the user's tokens from the database (revoke its bearer token)
            if (!string.IsNullOrWhiteSpace(userIdValue) && int.TryParse(userIdValue, out int userId))
            {
                await _tokenService.DeleteTokensForUserAsync(userId);
            }
            await _tokenService.DeleteExpiredTokensAsync();

            return true;
        }

        //[HttpGet("[action]"), HttpPost("[action]")]
        //public bool IsAuthenthenticated()
        //{
        //    return User.Identity.IsAuthenticated;
        //}

        //[HttpGet("[action]"), HttpPost("[action]")]
        //public IActionResult GetUserInfo()
        //{
        //    var claimsIdentity = User.Identity as ClaimsIdentity;
        //    return Json(new { Username = claimsIdentity.Name });
        //}
    }
}
