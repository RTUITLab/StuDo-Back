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

        public ConfirmEmailModel(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        //public Task<IActionResult> OnGet()
        //{
        //    return NotFound();
        //}

        public async Task<IActionResult> OnGetAsync (string userId, string token)
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest("User is null");

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return RedirectToPage("/Index");
            else
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }
    }
}