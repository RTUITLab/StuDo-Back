using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using studo.Models;
using studo.Models.Requests.AccountManagment;
using System.Threading.Tasks;

namespace studo.Controllers.Users
{
    /// <summary>
    /// Controller to manage user's accounts:
    /// --- Confirm email
    /// --- Reset password
    /// </summary>
    [Produces("application/json")]
    [Route("api/account/manage")]
    [ApiController]
    public class AccountManagmentController : Controller
    {
        private readonly UserManager<User> userManager;

        public AccountManagmentController(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        /// <summary>
        /// Confirm email request
        /// </summary>
        /// <param name="confirmEmailRequest"></param>
        /// <returns>All is ok</returns>
        /// <response code="200">Email was successfully confirmed</response>
        /// <response code="400">Invalid token or user</response>
        [AllowAnonymous]
        [HttpPost("confirmEmail")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ConfirmEmailRequestAsync([FromBody] ConfirmEmailRequestRequest confirmEmailRequest)
        {
            var user = await userManager.FindByIdAsync(confirmEmailRequest.UserId.ToString());
            if (user == null)
                return NotFound($"Can't find user with id --- {confirmEmailRequest.UserId}");

            if (!string.IsNullOrEmpty(confirmEmailRequest.NewEmail))
            {
                var result = await userManager.ChangeEmailAsync(user, confirmEmailRequest.NewEmail, confirmEmailRequest.Token);
                if (result.Succeeded)
                    return Ok();
            }
            else
            {
                var result = await userManager.ConfirmEmailAsync(user, confirmEmailRequest.Token);
                if (result.Succeeded)
                    return Ok();
            }
            return BadRequest("Token is invalid");
        }

        /// <summary>
        /// Reset password request
        /// </summary>
        /// <param name="resetPasswordRequest"></param>
        /// <returns>All is ok</returns>
        /// <response code="200">Password was reset successfully</response>
        /// <response code="400">Invalid token or user</response>
        /// <response code="404">User wasn't found</response>
        [AllowAnonymous]
        [HttpPost("resetPassword")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ResetPasswordRequestAsync([FromBody] ResetPasswordRequestRequest resetPasswordRequest)
        {
            var user = await userManager.FindByIdAsync(resetPasswordRequest.UserId.ToString());
            if (user == null)
                return NotFound($"Can't find user with id --- {resetPasswordRequest.UserId}");

            if (await userManager.CheckPasswordAsync(user, resetPasswordRequest.NewPassword))
                return BadRequest("New password can't match your previous password");

            var result = await userManager.ResetPasswordAsync(user, resetPasswordRequest.Token, resetPasswordRequest.NewPassword);
            if (result.Succeeded)
                return Ok();
            return BadRequest("Token is invalid");
        }
    }
}
