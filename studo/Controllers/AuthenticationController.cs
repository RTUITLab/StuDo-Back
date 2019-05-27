using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using studo.Models;
using studo.Models.Requests.Authentication;
using studo.Models.Responses.Authentication;
using studo.Models.Responses.Users;
using studo.Services.Autorize;
using studo.Services.Configure;

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

        public AuthenticationController(UserManager<User> userManager, IMapper mapper,
            ILogger<AuthenticationController> logger, IJwtFactory jwtFactory)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.logger = logger;
            this.jwtFactory = jwtFactory;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserRegistrationRequest userRegistrationRequest)
        {
            //userManager.Generate

            var user = await userManager.FindByEmailAsync(userRegistrationRequest.Email);
            if (user != null)
                return BadRequest("User with this email exists");

            user = mapper.Map<User>(userRegistrationRequest);
            user.EmailConfirmed = true;
            user.UserName = user.Email;

            var result = await userManager.CreateAsync(user, userRegistrationRequest.Password);
            
            // add default 'user' role to created user
            user = await userManager.FindByEmailAsync(user.Email);
            await userManager.AddToRoleAsync(user, RolesConstants.User);
            logger.LogDebug($"User {user.Email} was created and added to role - {RolesConstants.User}");

            if (result.Succeeded)
                return Ok();
            else
            {
                logger.LogError($"Result of creating user with email {user.Email} is {result}");
                throw new Exception($"Result of creating user with email {user.Email} is {result}");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] UserLoginRequest userLoginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await userManager.FindByEmailAsync(userLoginRequest.Email);
            if (user == null)
                return NotFound($"Can't find user with email {userLoginRequest.Email}");

            if (!await userManager.CheckPasswordAsync(user, userLoginRequest.Password))
                return BadRequest($"Incorrect password");

            if (!user.EmailConfirmed)
                return BadRequest($"Email {user.Email} isn't confirmed");

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

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Content("Hello world");
        }
    }
}
