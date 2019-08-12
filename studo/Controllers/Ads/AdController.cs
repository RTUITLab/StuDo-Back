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

        public AdController(UserManager<User> userManager, IAdManager adManager,
            IMapper mapper, ILogger<AdController> logger)
        {
            this.userManager = userManager;
            this.adManager = adManager;
            this.mapper = mapper;
            this.logger = logger;
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<IEnumerable<AdView>>> GetUsersAdsAsync(Guid userId)
        {
            var ads = adManager.Ads.Where(ad => ad.UserId.Value == userId);
            if (ads.Count() == 0)
                return Ok(new List<AdView>());

            return Ok(
                await ads
                .ProjectTo<AdView>(mapper.ConfigurationProvider)
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
            if (!adCreateRequest.OrganizationId.HasValue)
                adCreateRequest.UserId = Guid.Parse(userManager.GetUserId(User));

            var createdAd = await adManager.AddAsync(adCreateRequest);
            if (createdAd == null)
                return BadRequest("Can't create ad");

            AdView newAd = await createdAd
                .ProjectTo<AdView>(mapper.ConfigurationProvider)
                .SingleAsync();

            return Ok(newAd);
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
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message + "\n" + ae.StackTrace);
                return NotFound($"Can't find ad {adEditRequest.Id}");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {currentUserId} has no rights to edit organization {adEditRequest.Id}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpDelete]
        [Route("{adId:guid}")]
        public async Task<ActionResult<Guid>> DeleteAdAsync(Guid adId)
        {
            var ad = await adManager.Ads.FirstOrDefaultAsync(ev => ev.Id == adId);
            if (ad == null)
                return BadRequest("Can't find ad");

            var currentUserId = GetCurrentUserId();
            if (ad.UserId.HasValue && currentUserId != ad.UserId.Value)
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);

            await adManager.DeleteAsync(adId);
            return Ok(adId);
        }

        [HttpGet("{adId:guid}")]
        public async Task<ActionResult<AdView>> GetAdAsync(Guid adId)
            => await adManager.Ads
            .ProjectTo<AdView>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ad => ad.Id == adId);

        private Guid GetCurrentUserId()
            => Guid.Parse(userManager.GetUserId(User));
    }
}
