using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

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

            //var result = await signInManager.PasswordSignInAsync(user, userLoginRequest.Password, isPersistent: true, lockoutOnFailure: true);
            //if (result.Succeeded)
            //{
            //    return LocalRedirect(returnUrl);
            //}
            //await signInManager.SignInAsync(user, false);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var userRoles = await _manager.GetRolesAsync(user);
            claims.AddRange(userRoles.Select(name => new Claim(ClaimTypes.Role, name)));

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

            return LocalRedirect(returnUrl);
        }
    }
}