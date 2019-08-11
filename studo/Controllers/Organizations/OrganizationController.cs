using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationView>>> GetAllOrganizations()
        {
            if (organizationManager.Organizations.Count() == 0)
                return Ok(new List<OrganizationView>());

            return Ok(
                await organizationManager.Organizations
                .ProjectTo<OrganizationView>(mapper.ConfigurationProvider)
                .ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<OrganizationView>> CreateOrganizationAsync([FromBody] OrganizationCreateRequest organizationCreateRequest)
        {
            var current = await GetCurrentUser();
            var createdOrg = await organizationManager.AddAsync(organizationCreateRequest, current);
            if (createdOrg == null)
            {
                logger.LogDebug("Created organization = null");
                return BadRequest();
            }

            OrganizationView newOrg = await createdOrg
                .ProjectTo<OrganizationView>(mapper.ConfigurationProvider)
                .SingleAsync();

            return Ok(newOrg);
        }

        [HttpPut]
        public async Task<ActionResult<OrganizationView>> EditOrganizationAsync([FromBody] OrganizationEditRequest organizationEditRequest)
        {
            var current = await GetCurrentUser();
            try
            {
                var editedOrg = await organizationManager.EditAsync(organizationEditRequest, current.Id);
                return Ok(await editedOrg
                    .ProjectTo<OrganizationView>(mapper.ConfigurationProvider)
                    .SingleAsync());
            }
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message);
                logger.LogDebug(ae.StackTrace);
                return NotFound("Can't find organization");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message);
                logger.LogDebug(mae.StackTrace);
                logger.LogDebug($"User {current.Email} has no rights to edit organization {organizationEditRequest.Id}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message);
                logger.LogDebug(ex.StackTrace);
                return StatusCode(500);
            }
        }

        private async Task<User> GetCurrentUser()
            => await userManager.FindByIdAsync(userManager.GetUserId(User));
    }
}
