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
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<User> userManager;
        public bool IsOk { get; set; }


        public ConfirmEmailModel(UserManager<User> userManager)
        {
            this.userManager = userManager;
            IsOk = false;
        }

        public async Task<IActionResult> OnGetAsync (string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return NotFound();

            if (!ModelState.IsValid)
                return Page();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest("User is null");

            var result = await userManager.ConfirmEmailAsync(user, token);
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