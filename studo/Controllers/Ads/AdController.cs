using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
            => await adManager.Ads.OrderByDescending(ad => ad.EndTime)
                .ProjectTo<CompactAdView>(mapper.ConfigurationProvider)
                .ToListAsync();

        [HttpPost]
        // TODO: delete field userId and User from create request
        // check it here like in resume
        public async Task<ActionResult<AdView>> CreateAdAsync([FromBody] AdCreateRequest adCreateRequest)
        {
            if (adCreateRequest == null)
                return BadRequest("No data inside");

            if (!adCreateRequest.UserId.HasValue && !adCreateRequest.OrganizationId.HasValue ||
                adCreateRequest.UserId.HasValue && adCreateRequest.OrganizationId.HasValue)
                return BadRequest("No creator or more than one creator");

            AdView newAd = await (await adManager.AddAsync(adCreateRequest))
                .ProjectTo<AdView>(mapper.ConfigurationProvider)
                .SingleAsync();

            return Ok(newAd);
        }

        [HttpPut]
        public async Task<ActionResult<AdView>> EditAdAsync([FromBody] AdEditRequest adEditRequest)
        {
            var editedAd = await adManager.EditAsync(adEditRequest);
            if (editedAd == null)
                return NotFound(adEditRequest);

            return Ok(await editedAd.
                ProjectTo<AdView>(mapper.ConfigurationProvider).
                SingleAsync());
        }

        [Authorize(Roles = "admin")]
        [HttpDelete]
        [Route("{adId:guid}")]
        public async Task<ActionResult<Guid>> DeleteAdAsync(Guid adId)
        {
            await adManager.DeleteAsync(adId);
            return Ok(adId);
        }

        [HttpGet("{adId:guid}")]
        public async Task<ActionResult<AdView>> GetAdAsync(Guid adId)
            => await adManager.Ads
            .ProjectTo<AdView>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ad => ad.Id == adId);
    }
}
