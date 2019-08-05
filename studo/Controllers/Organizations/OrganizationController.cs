using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using studo.Models;
using studo.Models.Requests.Organization;
using studo.Models.Responses.Organization;
using studo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Controllers.Organizations
{
    [Produces("application/json")]
    [Route("api/organization")]
    public class OrganizationController : Controller
    {
        private readonly IMapper mapper;
        private readonly ILogger<OrganizationController> logger;
        private readonly IOrganizationManager organizationManager;
        private readonly UserManager<User> userManager;

        public OrganizationController(IMapper mapper, ILogger<OrganizationController> logger,
            IOrganizationManager organizationManager, UserManager<User> userManager)
        {
            this.mapper = mapper;
            this.logger = logger;
            this.organizationManager = organizationManager;
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult<OrganizationView>> CreateOrganizationAsync([FromBody] OrganizationCreateRequest organizationCreateRequest)
        {
            var current = await GetCurrentUser();
            var createdOrg = await organizationManager.AddAsync(organizationCreateRequest, current);
            if (createdOrg == null)
                return BadRequest();

            OrganizationView newOrg = await createdOrg
                .ProjectTo<OrganizationView>(mapper.ConfigurationProvider)
                .SingleAsync();

            return Ok(newOrg);
        }

        private async Task<User> GetCurrentUser()
            => await userManager.FindByIdAsync(userManager.GetUserId(User));
    }
}
