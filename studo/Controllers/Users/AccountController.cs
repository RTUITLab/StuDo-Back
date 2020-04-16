using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using studo.Models;
using studo.Models.Requests.Users;
using studo.Models.Responses.Users;
using studo.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace studo.Controllers.Users
{
    /// <summary>
    /// Controller for making operations with users account
    /// </summary>
    [Produces("application/json")]
    [Route("api/user")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly IEmailSender emailSender;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<User> userManager, IMapper mapper,
            IEmailSender emailSender, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="resetPasswordRequest"></param>
        /// <returns>All is ok</returns>
        /// <response code="200">Recover email was successfully sent</response>
        /// <response code="400">Can't find user</response>
        [AllowAnonymous]
        [HttpPost("password/reset")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPaswwordAsync ([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            var user = await userManager.FindByEmailAsync(resetPasswordRequest.Email);
            if (user == null)
                return BadRequest($"User with email {resetPasswordRequest.Email} doesn't exist");

            var resetPassToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"https://{Request.Host}/Logged#/PassReset?userId={user.Id}&token={resetPassToken}";

            await emailSender.SendResetPasswordEmail(resetPasswordRequest.Email, callbackUrl);
            return Ok();
        }

        /// <summary>
        /// Change user's password
        /// </summary>
        /// <param name="changePasswordRequest"></param>
        /// <returns>All is ok</returns>
        /// <response code="200">Password was changed</response>
        /// <response code="400">Old password doesn't match or new password can't match old password</response>
        /// <response code="404">User wasn't found</response>
        [HttpPost("password/change")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangePasswordAsync ([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            // TODO: IF IT WASN'T YOU PLEASE GO HERE TO RESET PASSWORD
            // send email that your password was reseted!!!
            var user = await userManager.FindByIdAsync(changePasswordRequest.Id.ToString());
            if (user == null)
                return NotFound($"Current user {changePasswordRequest.Id} doesn't exist");

            if (user.Id != GetCurrentUserId())
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);

            if (!await userManager.CheckPasswordAsync(user, changePasswordRequest.OldPassword))
                return BadRequest("Old password doesn't match current password");

            if (changePasswordRequest.NewPassword.Equals(changePasswordRequest.OldPassword))
                return BadRequest("New password can't match old password");

            var result = await userManager.ChangePasswordAsync(user, changePasswordRequest.OldPassword, changePasswordRequest.NewPassword);
            if (result.Succeeded)
            {
                // send email that password was reseted
                return Ok();
            }
            else
            {
                foreach (var er in result.Errors)
                {
                    logger.LogError($"Result of changing password for user with email {user.Email} is {er}");
                }
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Get information about one user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>One user information</returns>
        /// <response code="200">All is ok</response>
        /// <response code="404">Can't find user</response>
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserView>> GetOneUserAsync(Guid userId)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId.ToString())
                    ?? throw new ArgumentNullException();
                return Ok(mapper.Map<UserView>(user));
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound($"Can't find user");
            }
        }

        /// <summary>
        /// Change user's information
        /// </summary>
        /// <param name="changeUserInformationRequest"></param>
        /// <returns>New user's information</returns>
        /// <response code="200">All is ok. Information was changed</response>
        /// <response code="403">Current user can't change another's information</response>
        /// <response code="404">Can't find current user</response>
        [HttpPost("change/info")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserView>> ChangeUserInfromationAsync([FromBody] ChangeUserInformationRequest changeUserInformationRequest)
        {
            try
            {
                if (changeUserInformationRequest.Id != GetCurrentUserId())
                    throw new MethodAccessException();

                var currentUser = await userManager.FindByIdAsync(changeUserInformationRequest.Id.ToString())
                    ?? throw new ArgumentNullException();

                mapper.Map(changeUserInformationRequest, currentUser);
                var result = await userManager.UpdateAsync(currentUser);
                if (result.Succeeded)
                {
                    return Ok(mapper.Map<UserView>(currentUser));
                }
                else
                {
                    foreach (var er in result.Errors)
                    {
                        logger.LogError($"Result of changing user information is {er}");
                    }
                    return StatusCode(500);
                }
            }
            catch(ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound($"Can't find current user");
            }
            catch(MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {GetCurrentUserId()} can't change {changeUserInformationRequest.Id} user information");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }

        /// <summary>
        /// Change user's email
        /// </summary>
        /// <param name="changeEmailRequest"></param>
        /// <returns>Email to new address</returns>
        /// <response code="200">All is ok</response>
        /// <response code="400">New email can't match old email</response>
        /// <response code="403">Current user can't change another's email address</response>
        /// <response code="404">Can't find current user</response>
        [HttpPost("change/email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangeEmailAsync([FromBody] ChangeEmailRequest changeEmailRequest)
        {
            try
            {
                if (GetCurrentUserId() != changeEmailRequest.Id)
                    throw new MethodAccessException();

                bool exist = await userManager.Users.AnyAsync(u => u.Email == changeEmailRequest.NewEmail);
                if (exist)
                    throw new MethodAccessException();

                var currentUser = await userManager.FindByIdAsync(changeEmailRequest.Id.ToString())
                    ?? throw new ArgumentNullException();

                if (currentUser.Email != changeEmailRequest.OldEmail)
                    throw new ArgumentException();

                string emailConfirmationToken = await userManager.GenerateChangeEmailTokenAsync(currentUser, changeEmailRequest.NewEmail);
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = changeEmailRequest.Id, token = emailConfirmationToken, newEmail = changeEmailRequest.NewEmail },
                    protocol: "https");

                await emailSender.SendEmailConfirmationAsync(changeEmailRequest.NewEmail, callbackUrl);
                return Ok();
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound($"Can't find current user");
            }
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message + "\n" + ae.StackTrace);
                return BadRequest("Old email isn't match current user's email");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {GetCurrentUserId()} can't change {changeEmailRequest.Id} email to {changeEmailRequest.NewEmail}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }


        private Guid GetCurrentUserId()
            => Guid.Parse(userManager.GetUserId(User));
    }
}
