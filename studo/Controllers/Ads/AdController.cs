using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using studo.Models;
using studo.Models.Requests.Ads;
using studo.Models.Responses.Ads;
using studo.Services.Interfaces;

namespace studo.Controllers.Ads
{
    [Produces("application/json")]
    [Route("api/ad")]
    [ApiController]
    public class AdController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IAdManager adManager;
        private readonly IMapper mapper;
        private readonly ILogger<AdController> logger;
        private readonly IOrganizationManager organizationManager;

        public AdController(UserManager<User> userManager, IAdManager adManager,
            IMapper mapper, ILogger<AdController> logger, IOrganizationManager organizationManager)
        {
            this.userManager = userManager;
            this.adManager = adManager;
            this.mapper = mapper;
            this.logger = logger;
            this.organizationManager = organizationManager;
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<IEnumerable<CompactAdView>>> GetUserAdsAsync(Guid userId)
        {
            bool exist = await userManager.Users
                .Where(u => u.Id == userId)
                .AnyAsync();

            if (!exist)
            {
                logger.LogDebug($"Can't find user {userId}");
                return NotFound($"Can't find user {userId}");
            }

            var ads = adManager.Ads
                .Where(ad => ad.UserId.HasValue && ad.UserId.Value == userId);
            if (ads.Count() == 0)
                return Ok(new List<CompactAdView>());

            return Ok(
                await ads
                .ProjectTo<CompactAdView>(mapper.ConfigurationProvider)
                .ToListAsync());
        }

        [HttpGet("organization/{orgId:guid}")]
        public async Task<ActionResult<IEnumerable<CompactAdView>>> GetOrganizationAdsAsync(Guid orgId)
        {
            bool exist = await organizationManager.Organizations
                .Where(org => org.Id == orgId)
                .AnyAsync();

            if (!exist)
            {
                logger.LogDebug($"Can't find organization {orgId}");
                return NotFound($"Can't find organization {orgId}");
            }

            var ads = adManager.Ads
                .Where(ad => ad.OrganizationId.HasValue && ad.OrganizationId.Value == orgId);
            if (ads.Count() == 0)
                return Ok(new List<CompactAdView>());

            return Ok(
                await ads
                .ProjectTo<CompactAdView>(mapper.ConfigurationProvider)
                .ToListAsync());
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompactAdView>>> GetAdsAsync()
        {
            if (adManager.Ads.Count() == 0)
                return Ok(new List<CompactAdView>());

            return Ok(
                await adManager.Ads
                .OrderByDescending(ad => ad.EndTime)
                .ProjectTo<CompactAdView>(mapper.ConfigurationProvider)
                .ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<AdView>> CreateAdAsync([FromBody] AdCreateRequest adCreateRequest)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                var createdAd = await adManager.AddAsync(adCreateRequest, currentUserId);
                AdView newAd = await createdAd
                    .ProjectTo<AdView>(mapper.ConfigurationProvider)
                    .SingleAsync();

                return Ok(newAd);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                if (adCreateRequest.UserId.HasValue)
                {
                    logger.LogDebug($"Current user {currentUserId} doesn't exist in database");
                    return BadRequest($"Current user {currentUserId} doesn't exist in database");
                }
                else if (adCreateRequest.OrganizationId.HasValue)
                {
                    logger.LogDebug($"Organization {adCreateRequest.OrganizationId.Value} doesn't exist in database");
                    return BadRequest($"Organization {adCreateRequest.OrganizationId.Value} doesn't exist in database");
                }
                else
                {
                    logger.LogDebug("No userId or organizationId while creating ad");
                    return BadRequest("No userId or organizationId while creating ad");
                }
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"Current user {currentUserId} has no rights to create ads in organization {adCreateRequest.OrganizationId.Value}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPut]
        public async Task<ActionResult<AdView>> EditAdAsync([FromBody] AdEditRequest adEditRequest)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                var editedAd = await adManager.EditAsync(adEditRequest, currentUserId);
                return Ok(await editedAd
                    .ProjectTo<AdView>(mapper.ConfigurationProvider)
                    .SingleAsync());
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound($"Can't find ad {adEditRequest.Id}");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"Current user {currentUserId} has no rights to edit ad {adEditRequest.Id}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpDelete("{adId:guid}")]
        public async Task<ActionResult<Guid>> DeleteAdAsync(Guid adId)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                await adManager.DeleteAsync(adId, currentUserId);
                return Ok(adId);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound($"Can't find ad {adId}");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {currentUserId} has no rights to delete ad {adId}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpGet("{adId:guid}")]
        public async Task<ActionResult<AdView>> GetAdAsync(Guid adId)
        {
            try
            {
                AdView adView = await adManager.Ads
                    .ProjectTo<AdView>(mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(ad => ad.Id == adId)
                    ?? throw new ArgumentException();
                return Ok(adView);
            }
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message + "\n" + ae.StackTrace);
                return NotFound($"Can't find ad {adId}");
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        private Guid GetCurrentUserId()
            => Guid.Parse(userManager.GetUserId(User));
    }
}
