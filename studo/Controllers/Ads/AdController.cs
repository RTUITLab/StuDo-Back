using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using studo.Models;
using studo.Models.Requests.Ads;
using studo.Models.Responses.Ads;
using studo.Services.Interfaces;

namespace studo.Controllers.Ads
{
    [Produces("application/json")]
    [Route("api/ad")]
    public class AdController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IAdManager adManager;
        private readonly IMapper mapper;

        public AdController(UserManager<User> userManager, IAdManager adManager,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.adManager = adManager;
            this.mapper = mapper;
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
        // TODO: delete field userId and User from create request
        // check it here like in resume
        public async Task<ActionResult<AdView>> CreateAdAsync([FromBody] AdCreateRequest adCreateRequest)
        {
            if (adCreateRequest == null)
                return BadRequest("No data inside");

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
            var ad = await adManager.Ads.FirstOrDefaultAsync(ev => ev.Id == adEditRequest.Id);
            if (ad == null)
                return BadRequest("Can't find ad");

            var current = await GetCurrentUser();
            if (current.Id != ad.UserId)
                return Forbid(JwtBearerDefaults.AuthenticationScheme);

            var editedAd = await adManager.EditAsync(adEditRequest);
            if (editedAd == null)
                return NotFound(adEditRequest);

            return Ok(await editedAd.
                ProjectTo<AdView>(mapper.ConfigurationProvider).
                SingleAsync());
        }

        [HttpDelete]
        [Route("{adId:guid}")]
        public async Task<ActionResult<Guid>> DeleteAdAsync(Guid adId)
        {
            var ad = await adManager.Ads.FirstOrDefaultAsync(ev => ev.Id == adId);
            if (ad == null)
                return BadRequest("Can't find ad");

            var current = await GetCurrentUser();
            if (current.Id != ad.UserId.Value)
                return Forbid(JwtBearerDefaults.AuthenticationScheme);

            await adManager.DeleteAsync(adId);
            return Ok(adId);
        }

        [HttpGet("{adId:guid}")]
        public async Task<ActionResult<AdView>> GetAdAsync(Guid adId)
            => await adManager.Ads
            .ProjectTo<AdView>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ad => ad.Id == adId);

        private async Task<User> GetCurrentUser()
            => await userManager.FindByIdAsync(userManager.GetUserId(User));
    }
}
