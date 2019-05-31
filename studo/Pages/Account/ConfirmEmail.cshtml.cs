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

        //[BindProperty(SupportsGet = true)]
        //public Guid UserId { get; set; }

        //[BindProperty(SupportsGet = true)]
        //public string Token { get; set; }

        public ConfirmEmailModel(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync (string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return NotFound();

            if (!ModelState.IsValid)
                return Page();

            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return BadRequest("User is null");

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                // TODO: redirect to page "You have confirmed your email -> Login now"
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