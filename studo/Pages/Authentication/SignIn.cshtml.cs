using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SignInModel : PageModel
    {
        [BindProperty]
        public UserLoginRequest userLoginRequest { get; set; }

        private readonly UserManager<User> _manager;
        private readonly SignInManager<User> signInManager;

        public SignInModel(UserManager<User> manager, SignInManager<User> signInManager)
        {
            _manager = manager;
            this.signInManager = signInManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _manager.FindByEmailAsync(userLoginRequest.Email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(userLoginRequest.Email), "Incorrect email");
                return Page();
            }

            if (!await _manager.CheckPasswordAsync(user, userLoginRequest.Password))
            {
                ModelState.AddModelError(nameof(userLoginRequest.Password), "Incorrect password");
                return Page();
            }

            await signInManager.SignInAsync(user, false);

            return RedirectToPage("/Index");
        }
    }
}