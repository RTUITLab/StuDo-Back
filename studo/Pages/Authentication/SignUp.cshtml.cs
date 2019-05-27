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

namespace studo.Pages.Authentication
{
    [AllowAnonymous]
    public class SignUpModel : PageModel
    {
        [BindProperty]
        public UserRegistrationRequest userRegistrationRequest { get; set; }

        private readonly UserManager<User> manager;
        private readonly IMapper mapper;

        public SignUpModel(UserManager<User> manager, IMapper mapper)
        {
            this.manager = manager;
            this.mapper = mapper;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(userRegistrationRequest.Password) || !await FindUserAsync())
            {
                ModelState.AddModelError(nameof(userRegistrationRequest.Email), $"User with '{userRegistrationRequest.Email}' email is already exist");
                return Page();
            }

            var user = mapper.Map<User>(userRegistrationRequest);

            user.EmailConfirmed = true;
            user.UserName = user.Email;
            var result = await manager.CreateAsync(user, userRegistrationRequest.Password);

            if (result.Succeeded)
                return RedirectToPage("/Index");
            else
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        // return true if user with this Email doesn't exist
        private async Task<bool> FindUserAsync()
        {
            var user = await manager.FindByEmailAsync(userRegistrationRequest.Email);
            if (user == null)
                return await Task.FromResult(true);
            else
                return await Task.FromResult(false);
        }
    }
}