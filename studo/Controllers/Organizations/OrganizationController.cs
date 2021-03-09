using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using studo.Extensions;
using studo.Models;
using studo.Models.Requests.Organization;
using studo.Models.Responses.Organization;
using studo.Services.Configure;
using studo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Controllers.Organizations
{
    [Produces("application/json")]
    [Route("api/organization")]
    [ApiController]
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

        [HttpGet("members/{orgId:guid}")]
        public async Task<ActionResult<IEnumerable<MemberView>>> GetAllOrganizationsUsers(Guid orgId)
        {
            var wisherRight = OrganizationRights.Wisher.ToString();
            var users = (await organizationManager.Organizations
                .Where(org => org.Id == orgId)
                .SelectMany(org => org.Users)
                .Include(u => u.User)
                .Include(u => u.UserOrganizationRight)
                .Where(u => u.UserOrganizationRight.RightName != wisherRight)
                .ToListAsync())
                .GroupBy(u => u.User)
                .Select(g => mapper.Map<MemberView>(g))
                .ToList();

            return Ok(users);
        }

        [HttpGet("{orgId:guid}")]
        public async Task<ActionResult<OrganizationView>> GetOneOrganizationAsync(Guid orgId)
        {
            try
            {
                OrganizationView orgView = await organizationManager.Organizations
                    .ProjectTo<OrganizationView>(mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(org => org.Id == orgId)
                    ?? throw new ArgumentNullException();
                return Ok(orgView);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound($"Can't find organization {orgId}");
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationView>>> GetAllOrganizations(bool canPublish, bool member)
        {
            return Ok(
                await organizationManager.Organizations
                    .If(canPublish, orgs => orgs
                        .Where(org => org.Users.Any(user => user.UserOrganizationRight.RightName == OrganizationRights.CanEditAd.ToString() && user.UserId == GetCurrentUserId())))
                    .If(member, orgs => orgs
                        .Where(org => org.Users.Any(user => user.UserOrganizationRight.RightName == OrganizationRights.Member.ToString() && user.UserId == GetCurrentUserId())))
                    .ProjectTo<OrganizationView>(mapper.ConfigurationProvider)
                    .ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<OrganizationView>> CreateOrganizationAsync([FromBody] OrganizationCreateRequest organizationCreateRequest)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                var createdOrg = await organizationManager.AddAsync(organizationCreateRequest, currentUserId);
                OrganizationView newOrg = await createdOrg
                    .ProjectTo<OrganizationView>(mapper.ConfigurationProvider)
                    .SingleAsync();

                return Ok(newOrg);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                logger.LogDebug($"User {currentUserId} doesn't exist in current database");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message + "\n" + ae.StackTrace);
                logger.LogDebug($"Organization with name '{organizationCreateRequest.Name}' already exists");
                return BadRequest($"Organization with name '{organizationCreateRequest.Name}' already exists");
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPut]
        public async Task<ActionResult<OrganizationView>> EditOrganizationAsync([FromBody] OrganizationEditRequest organizationEditRequest)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                var editedOrg = await organizationManager.EditAsync(organizationEditRequest, currentUserId);
                return Ok(await editedOrg
                    .ProjectTo<OrganizationView>(mapper.ConfigurationProvider)
                    .SingleAsync());
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound($"Can't find organization {organizationEditRequest.Id}");
            }
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message + "\n" + ae.StackTrace);
                return BadRequest($"Organization with name '{organizationEditRequest.Name}' already exists");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {currentUserId} has no rights to edit organization {organizationEditRequest.Id}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpDelete("{orgId:guid}")]
        public async Task<ActionResult<Guid>> DeleteOrganizationAsync(Guid orgId)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                await organizationManager.DeleteAsync(orgId, currentUserId);
                return Ok(orgId);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound($"Can't find organization {orgId}");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {currentUserId} has no rights to delete organization {orgId}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPost("right/attach")]
        public async Task<IActionResult> AttachToOrganizationRightAsync([FromBody] AttachDetachRightRequest attachDetachRightRequest)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                await organizationManager.AttachToRightAsync(attachDetachRightRequest, currentUserId);
                return Ok();
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound("Can't find organization, user or right");
            }
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message + "\n" + ae.StackTrace);
                logger.LogDebug($"Can't attacth user {attachDetachRightRequest.UserId} to right {attachDetachRightRequest.Right}");
                return BadRequest("Can't attach to this right");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {currentUserId} has no rights to edit rights in organization {attachDetachRightRequest.OrganizationId}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (MemberAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {attachDetachRightRequest.UserId} already has right {attachDetachRightRequest.Right} in organization {attachDetachRightRequest.OrganizationId}");
                return BadRequest("User already has this right in organization");
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPost("right/detach")]
        public async Task<IActionResult> DetachFromOrganizationRightAsync([FromBody] AttachDetachRightRequest attachDetachRightRequest)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                await organizationManager.DetachFromRightAsync(attachDetachRightRequest, currentUserId);
                return Ok();
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                logger.LogDebug($"Can't find organization {attachDetachRightRequest.OrganizationId} or user {attachDetachRightRequest.UserId}");
                return NotFound("Can't find organization, user or right");
            }
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message + "\n" + ae.StackTrace);
                logger.LogDebug($"Can't detach user {attachDetachRightRequest.UserId} from right {attachDetachRightRequest.Right}");
                return BadRequest("Can't detach from this right");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {currentUserId} has no rights to edit rights in organization {attachDetachRightRequest.OrganizationId}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (MemberAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {attachDetachRightRequest.UserId} doesn't have right {attachDetachRightRequest.Right} in organization {attachDetachRightRequest.OrganizationId}");
                return BadRequest("User doesn't have this right in organization");
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPost("wish/{orgId:guid}")]
        public async Task<IActionResult> AddToWisherRight(Guid orgId)
        {
            try
            {
                await organizationManager.AddToWishers(orgId, GetCurrentUserId());
                return Ok();
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound("Can't find organization, user or right");
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogDebug(ioe.Message + "\n" + ioe.StackTrace);
                return NotFound("Can't find organization, user or right");
            }
            catch(Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpGet("wish/{orgId:guid}")]
        public async Task<ActionResult<IEnumerable<MemberView>>> GetAllOrganizationsWishers(Guid orgId)
        {
            var wisherRight = OrganizationRights.Wisher.ToString();
            var users = (await organizationManager.Organizations
                .Where(org => org.Id == orgId)
                .SelectMany(org => org.Users)
                .Include(u => u.User)
                .Include(u => u.UserOrganizationRight)
                .Where(u => u.UserOrganizationRight.RightName == wisherRight)
                .ToListAsync())
                .GroupBy(u => u.User)
                .Select(g => mapper.Map<MemberView>(g))
                .ToList();

            return Ok(users);
        }

        private Guid GetCurrentUserId()
            => Guid.Parse(userManager.GetUserId(User));
    }
}
