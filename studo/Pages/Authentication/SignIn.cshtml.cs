using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using studo.Models;
using studo.Models.Requests.Authentication;

namespace studo.Pages.Authentication
{
    public class SignInModel : PageModel
    {
        [BindProperty]
        public UserLoginRequest userLoginRequest { get; set; }

        private readonly UserManager<User> _manager;

        public SignInModel(UserManager<User> manager)
        {
            _manager = manager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // TODO: reload this page and show an error
            var user = await _manager.FindByEmailAsync(userLoginRequest.Email) ??
                throw new Exception("Incorrect email");

            if (!await _manager.CheckPasswordAsync(user, userLoginRequest.Password))
                throw new Exception("Incorrect password");

            return RedirectToPage("/Index");
        }
    }
}