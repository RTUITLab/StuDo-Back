using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using studo.Models;
using studo.Models.Requests.Authentication;
using studo.Models.Responses.Authentication;
using studo.Models.Responses.Users;
using studo.Services.Autorize;
using studo.Services.Configure;
using studo.Services.Interfaces;

namespace studo.Controllers
{
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

        public AuthenticationController(UserManager<User> userManager, IMapper mapper,
            ILogger<AuthenticationController> logger, IJwtFactory jwtFactory, IEmailSender emailSender, IHostingEnvironment env)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.logger = logger;
            this.jwtFactory = jwtFactory;
            this.emailSender = emailSender;
            this.env = env;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserRegistrationRequest userRegistrationRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest();

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
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = user.Id, token = emailConfirmationToken},
                    protocol: "https");

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

        [AllowAnonymous]
        [HttpPost("login")]
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
                AccessToken = jwtFactory.GenerateAccessToken(user.Id, userRoles)
            };
        }

        [Authorize(Roles = RolesConstants.Admin)]
        [HttpDelete("{userEmail}")]
        public async Task<ActionResult<string>> DeleteUserAsync(string userEmail)
        {
            if (!env.IsDevelopment())
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);

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

                throw new Exception($"Result of deletign user with email {user.Email} is {result}");
            }
        }
    }
}
