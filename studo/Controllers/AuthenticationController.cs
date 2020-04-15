using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using studo.Data;
using studo.Models;
using studo.Models.Options;
using studo.Models.Requests.Authentication;
using studo.Models.Responses.Authentication;
using studo.Models.Responses.Users;
using studo.Services.Autorize;
using studo.Services.Configure;
using studo.Services.Interfaces;

namespace studo.Controllers
{
    /// <summary>
    /// Controller for login and register users
    /// </summary>
    [Produces("application/json")]
    [Route("api/auth")]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly ILogger<AuthenticationController> logger;
        private readonly IJwtFactory jwtFactory;
        private readonly IEmailSender emailSender;
        private readonly IHostingEnvironment env;
        private readonly DatabaseContext context;
        private readonly TimeSpan RefreshTokenLifeTime;

        public AuthenticationController(UserManager<User> userManager, IMapper mapper,
            ILogger<AuthenticationController> logger, IJwtFactory jwtFactory,
            IEmailSender emailSender, IHostingEnvironment env, DatabaseContext context,
            IOptions<JwtOptions> jwtOptions)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.logger = logger;
            this.jwtFactory = jwtFactory;
            this.emailSender = emailSender;
            this.env = env;
            this.context = context;
            RefreshTokenLifeTime = jwtOptions.Value.RefreshTokenLifeTime;
        }

        /// <summary>
        /// Creates a new user and give him 'user' role
        /// </summary>
        /// <param name="userRegistrationRequest"></param>
        /// <returns>All is ok</returns>
        /// <response code="200">If user was successfully created and postcard to confirm email was sent</response>
        /// <response code="400">If user with such email exists</response>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterAsync([FromBody] UserRegistrationRequest userRegistrationRequest)
        {
            var user = await userManager.FindByEmailAsync(userRegistrationRequest.Email);
            if (user != null)
                return BadRequest("User with this email exists");

            user = mapper.Map<User>(userRegistrationRequest);
            user.UserName = user.Email;

            var result = await userManager.CreateAsync(user, userRegistrationRequest.Password);

            // add default 'user' role to created user
            user = await userManager.FindByEmailAsync(user.Email);
            await userManager.AddToRoleAsync(user, RolesConstants.User);
            logger.LogDebug($"User {user.Email} was created and added to role - {RolesConstants.User}");

            if (result.Succeeded)
            {
                var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = $"https://{Request.Host}/Logged#/Acceptation?userId={user.Id}&token={emailConfirmationToken}";

                await emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl);

                return Ok();
            }
            else
            {
                foreach (var er in result.Errors)
                    logger.LogError($"Result of creating user with email {user.Email} is {er}");

                throw new Exception($"Result of creating user with email {user.Email} is {result}");
            }
        }

        /// <summary>
        /// Login user to a system
        /// </summary>
        /// <param name="userLoginRequest"></param>
        /// <returns>User info and his access token</returns>
        /// <response code="200">If user exists and password is correct</response>
        /// <response code="400">If user's email isn't confirmed or incorrent password</response>
        /// <response code="404">If user with this email doesn't exist</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] UserLoginRequest userLoginRequest)
        {
            var user = await userManager.FindByEmailAsync(userLoginRequest.Email);
            if (user == null)
                return NotFound($"Can't find user with email {userLoginRequest.Email}");

            if (!user.EmailConfirmed)
                return BadRequest("User's email isn't confirmed");

            if (!await userManager.CheckPasswordAsync(user, userLoginRequest.Password))
                return BadRequest($"Incorrect password");

            var loginResponse = await GetLoginResponseAsync(user);
            return Ok(loginResponse);
        }

        private async Task<LoginResponse> GetLoginResponseAsync(User user)
        {
            var userRoles = await userManager.GetRolesAsync(user);

            return new LoginResponse
            {
                User = mapper.Map<UserView>(user),
                AccessToken = jwtFactory.GenerateAccessToken(user.Id, userRoles),
                RefreshToken = await SaveAndGetRefreshTokenAsync(user)
            };
        }

        [Authorize(Roles = RolesConstants.Admin)]
        [HttpDelete("{userEmail}")]
        public async Task<ActionResult<string>> DeleteUserAsync(string userEmail)
        {
            if (!env.IsDevelopment())
            {
                logger.LogError($"Environment is {env.EnvironmentName}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var user = await userManager.FindByEmailAsync(userEmail);
            if (user == null)
                return NotFound(userEmail);

            if (user.EmailConfirmed)
            {
                logger.LogError($"{userEmail} email is 'confirmed'");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }

            if (user.Ads != null && user.Ads.Count > 0)
            {
                logger.LogError($"{userEmail} Ads count - {user.Ads.Count}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }

            if (user.Organizations != null && user.Organizations.Count > 0)
            {
                logger.LogError($"{userEmail} UserRightsInOrganizations count - {user.Organizations.Count}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Ok(userEmail);
            else
            {
                foreach (var er in result.Errors)
                    logger.LogError($"Result of deleting user with email {user.Email} is {er}");

                throw new Exception($"Result of deleting user with email {user.Email} is {result}");
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> RefreshAsync([FromBody] RefreshTokenRequest request)
        {
            var dbToken = await context.RefreshTokens
                .Where(rt => rt.Token == request.RefreshToken)
                .SingleOrDefaultAsync();

            if (dbToken == null)
                return NotFound("Refresh token couldn't be found.");

            var user = await ReadUserFromRefreshToken(dbToken.Token);
            context.RefreshTokens.Remove(dbToken);
            await context.SaveChangesAsync();

            return Ok(
                await GetLoginResponseAsync(user)
            );
        }

        private async Task<string> SaveAndGetRefreshTokenAsync(User user)
        {
            var token = jwtFactory.GenerateRefreshToken(user.Id);
            var refreshToken = new RefreshToken
            {
                User = user,
                UserId = user.Id,
                Token = token,
                ExpireTime = DateTime.UtcNow + RefreshTokenLifeTime
            };
            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();
            return token;
        }

        private async Task<User> ReadUserFromRefreshToken(string token)
        {
            var decodedJwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);
            var claim = decodedJwt.Claims.ToList().FirstOrDefault(cl => cl.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return await userManager.FindByIdAsync(claim.Value);
        }

        private Guid GetCurrentUserId()
            => Guid.Parse(userManager.GetUserId(User));
    }
}
