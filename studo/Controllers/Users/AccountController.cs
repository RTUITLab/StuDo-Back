using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using studo.Models;
using studo.Models.Requests.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Controllers.Users
{
    [Produces("application/json")]
    [Route("api/auth")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> manager;
        private readonly IMapper mapper;

        public AccountController(UserManager<User> manager, IMapper mapper)
        {
            this.manager = manager;
            this.mapper = mapper;
        }

        //[AllowAnonymous]
        //[HttpPost("password/reset")]
        //public async Task<IActionResult> ResetPaswword([FromBody] ResetPasswordRequest resetPasswordRequest)
        //{
        //    var user = await manager.FindByEmailAsync(resetPasswordRequest.Email);
        //    if (user == null)
        //        return BadRequest($"User with email {resetPasswordRequest.Email} doesn't exist");

        //    var result = await manager.ResetPasswordAsync(user, )
        //}
    }
}
