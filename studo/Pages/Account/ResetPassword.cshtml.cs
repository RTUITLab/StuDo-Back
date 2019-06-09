using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using studo.Models;

namespace studo.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private static string Token;
        private static User User;

        [BindProperty]
        public string NewPassword { get; set; }
        [BindProperty]
        public string NewPasswordConfirm { get; set; }
        public bool IsOk { get; set; }

        public ResetPasswordModel(UserManager<User> userManager)
        {
            this.userManager = userManager;
            IsOk = false;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return NotFound();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest("User is null");

            User = user;
            Token = token;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!NewPassword.Equals(NewPasswordConfirm))
            {
                ModelState.AddModelError(NewPassword, "Passwords doesn't match");
                return Page();
            }

            var user = await userManager.FindByEmailAsync(User.Email);
            if(await userManager.CheckPasswordAsync(user, NewPasswordConfirm))
            {
                ModelState.AddModelError(NewPassword, "New password can't equals your previous password");
                return Page();
            }

            var result = await userManager.ResetPasswordAsync(user, Token, NewPassword);
            if (result.Succeeded)
                IsOk = true;
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}