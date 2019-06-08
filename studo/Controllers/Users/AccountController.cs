using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using studo.Models;
using studo.Models.Requests.Users;
using studo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Controllers.Users
{
    [Produces("application/json")]
    [Route("api/user")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly IEmailSender emailSender;

        public AccountController(UserManager<User> userManager, IMapper mapper,
            IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.emailSender = emailSender;
        }

        [AllowAnonymous]
        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPaswword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            var user = await userManager.FindByEmailAsync(resetPasswordRequest.Email);
            if (user == null)
                return BadRequest($"User with email {resetPasswordRequest.Email} doesn't exist");

            var resetPassToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { userId = user.Id, token = resetPassToken },
                protocol: Request.Scheme);

            await emailSender.SendResetPasswordEmail(resetPasswordRequest.Email, callbackUrl);
            return Ok();
        }
    }
}
