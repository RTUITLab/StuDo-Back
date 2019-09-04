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
        private static new User User;

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
            {

                return BadRequest("User is null");
            }

            User = user;
            Token = token;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!NewPassword.Equals(NewPasswordConfirm))
            {
                ModelState.AddModelError(nameof(NewPassword), "Passwords doesn't match");
                return Page();
            }

            if(await userManager.CheckPasswordAsync(User, NewPasswordConfirm))
            {
                ModelState.AddModelError(nameof(NewPassword), "New password can't equals your previous password");
                return Page();
            }

            var result = await userManager.ResetPasswordAsync(User, Token, NewPassword);
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