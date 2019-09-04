using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using studo.Models;
using studo.Models.Requests.Authentication;
using studo.Services.Interfaces;

namespace studo.Pages.Authentication
{
    [AllowAnonymous]
    public class SignUpModel : PageModel
    {
        [BindProperty]
        public UserRegistrationRequest userRegistrationRequest { get; set; }

        public bool IsCreated { get; set; }

        private readonly UserManager<User> userManager;
        private readonly IMapper mapper;
        private readonly IEmailSender emailSender;

        public SignUpModel(UserManager<User> userManager, IMapper mapper,
            IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.emailSender = emailSender;
            IsCreated = false;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (!await FindUserAsync())
            {
                ModelState.AddModelError(nameof(userRegistrationRequest.Email), $"User with '{userRegistrationRequest.Email}' email is already exist");
                return Page();
            }

            var user = mapper.Map<User>(userRegistrationRequest);

            user.UserName = user.Email;
            var result = await userManager.CreateAsync(user, userRegistrationRequest.Password);

            if (result.Succeeded)
            {
                var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = user.Id, token = emailConfirmationToken },
                    protocol: Request.Scheme);
                await emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl);

                IsCreated = true;
            }
            else
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        // return true if user with this Email doesn't exist
        private async Task<bool> FindUserAsync()
        {
            var user = await userManager.FindByEmailAsync(userRegistrationRequest.Email);
            if (user == null)
                return await Task.FromResult(true);
            else
                return await Task.FromResult(false);
        }
    }
}