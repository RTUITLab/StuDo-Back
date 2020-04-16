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
using studo.Extensions;
using studo.Models;
using studo.Models.Requests.Ads;
using studo.Models.Responses.Ads;
using studo.Services.Interfaces;

namespace studo.Controllers.Ads
{
    /// <summary>
    /// Controller for making operations with ads:
    /// Create, edit, delete, get all, get one, get user's/organization's ads
    /// </summary>
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

        /// <summary>
        /// Get all user's ads
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>All user's ads</returns>
        /// <response code="200">If userId is correct</response>
        /// <response code="404">If user with passed id doesn't exist</response>
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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
                .AttachCurrentUserId(mapper.ConfigurationProvider, GetCurrentUserId())
                .ProjectTo<CompactAdView>(mapper.ConfigurationProvider)
                .ToListAsync());
        }

        /// <summary>
        /// Get all organization's ads
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns>All organization's ads</returns>
        /// <response code="200">If organizationId is correct</response>
        /// <response code="404">If organization with passed id doesn't exist</response>
        [HttpGet("organization/{orgId:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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
                .AttachCurrentUserId(mapper.ConfigurationProvider, GetCurrentUserId())
                .ProjectTo<CompactAdView>(mapper.ConfigurationProvider)
                .ToListAsync());
        }

        /// <summary>
        /// Get all ads
        /// </summary>
        /// <returns>All ads</returns>
        /// <response code="200">All is correct</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<CompactAdView>>> GetAdsAsync()
        {
            if (adManager.Ads.Count() == 0)
                return Ok(new List<CompactAdView>());

            var now = DateTime.UtcNow;
            return Ok(
                await adManager.Ads
                    .Where(ad => ad.EndTime > now)
                    .OrderByDescending(ad => ad.CreationTime)
                    .AttachCurrentUserId(mapper.ConfigurationProvider, GetCurrentUserId())
                    .ProjectTo<CompactAdView>(mapper.ConfigurationProvider)
                    .ToListAsync());
        }

        /// <summary>
        /// Create ad
        /// </summary>
        /// <param name="adCreateRequest"></param>
        /// <returns>Created ad in a single view</returns>
        /// <response code="200">Create ad and attach it to current user. Or attach ad to passed organization</response>
        /// <response code="403">Current user has no rights to create ads in passed organization</response>
        /// <response code="404">Not such user or organization found</response>
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AdView>> CreateAdAsync([FromBody] AdCreateRequest adCreateRequest)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                var createdAd = await adManager.AddAsync(adCreateRequest, currentUserId);
                AdView newAd = await createdAd
                    .AttachCurrentUserId(mapper.ConfigurationProvider, GetCurrentUserId())
                    .ProjectTo<AdView>(mapper.ConfigurationProvider)
                    .SingleAsync();

                return Ok(newAd);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                if (adCreateRequest.OrganizationId.HasValue)
                {
                    logger.LogDebug($"Organization {adCreateRequest.OrganizationId.Value} doesn't exist in database");
                    return NotFound($"Organization {adCreateRequest.OrganizationId.Value} doesn't exist in database");
                }
                else
                {
                    logger.LogDebug($"Current user {currentUserId} doesn't exist in database");
                    return NotFound($"Current user {currentUserId} doesn't exist in database");
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

        /// <summary>
        /// Edit ad
        /// </summary>
        /// <param name="adEditRequest"></param>
        /// <returns>Edited ad in a single view</returns>
        /// <response code="200">If all is ok: user has rights to edit ad (he has rights in organization or it is his ad)</response>
        /// <response code="403">User tries to edit ad which doesn't belong to him or he has no rights in organization</response>
        /// <response code="404">Passed ad doesn't exist in database</response>
        [HttpPut]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AdView>> EditAdAsync([FromBody] AdEditRequest adEditRequest)
        {
            var currentUserId = GetCurrentUserId();
            try
            {
                var editedAd = await adManager.EditAsync(adEditRequest, currentUserId);
                return Ok(await editedAd
                    .AttachCurrentUserId(mapper.ConfigurationProvider, GetCurrentUserId())
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

        /// <summary>
        /// Delete ad
        /// </summary>
        /// <param name="adId"></param>
        /// <returns>Ad's id, which was deleted</returns>
        /// <response code="200">If user has rights (it's his ad or he's rights in organization)</response>
        /// <response code="403">If current user tries to delete foreign ad or has no rights in organization</response>
        /// <response code="404">Passed ad doesn't exist in database</response>
        [HttpDelete("{adId:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
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

        /// <summary>
        /// Get one ad
        /// </summary>
        /// <param name="adId"></param>
        /// <returns>One ad</returns>
        /// <response code="200">If all is okay</response>
        /// <response code="404">If ad wasn't found</response>
        [HttpGet("{adId:guid}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AdView>> GetAdAsync(Guid adId)
        {
            try
            {
                AdView adView = await adManager.Ads
                    .AttachCurrentUserId(mapper.ConfigurationProvider, GetCurrentUserId())
                    .ProjectTo<AdView>(mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(ad => ad.Id == adId)
                    ?? throw new ArgumentNullException();
                return Ok(adView);
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound($"Can't find ad {adId}");
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpGet("bookmarks")]
        public async Task<ActionResult<IEnumerable<CompactAdView>>> GetAllBookmarks()
        {
            return Ok(
                await adManager.Ads
                .OrderByDescending(ad => ad.EndTime)
                .Where(ad => ad.Bookmarks.Any(b => b.UserId == GetCurrentUserId()))
                .AttachCurrentUserId(mapper.ConfigurationProvider, GetCurrentUserId())
                .ProjectTo<CompactAdView>(mapper.ConfigurationProvider)
                .ToListAsync());
        }

        [HttpPost("bookmarks/{adId:guid}")]
        public async Task<IActionResult> AddToBookmarks(Guid adId)
        {
            try
            {
                await adManager.AddToBookmarks(adId, GetCurrentUserId());
                return Ok();
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogDebug(ioe.Message + "\n" + ioe.StackTrace);
                return NotFound("Can't find ad or user");
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound("Can't find ad ot user");
            }
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message + "\n" + ae.StackTrace);
                return BadRequest("Already in bookmarks");
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpDelete("bookmarks/{adId:guid}")]
        public async Task<IActionResult> RemoveFromBookmarks(Guid adId)
        {
            try
            {
                await adManager.RemoveFromBookmarks(adId, GetCurrentUserId());
                return Ok();
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogDebug(ioe.Message + "\n" + ioe.StackTrace);
                return NotFound("Can't find ad or user");
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound("Can't find ad ot user");
            }
            catch (ArgumentException ae)
            {
                logger.LogDebug(ae.Message + "\n" + ae.StackTrace);
                return BadRequest("Already not in bookmarks");
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpPost("comment/{adId:guid}")]
        public async Task<IActionResult> AddComment(Guid adId, [FromBody] AdCommentRequest adCommentRequest)
        {
            try
            {
                await adManager.AddComment(adId, GetCurrentUserId(), adCommentRequest);
                return Ok();
            }
            catch(InvalidOperationException ioe)
            {
                logger.LogDebug(ioe.Message + "\n" + ioe.StackTrace);
                return NotFound("Can't find ad or user");
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound("Can't find ad ot user");
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex.Message + "\n" + ex.StackTrace);
                return StatusCode(500);
            }
        }

        [HttpDelete("comment/{adId:guid}/{commentId:guid}")]
        public async Task<IActionResult> DeleteComment(Guid adId, Guid commentId)
        {
            try
            {
                await adManager.DeleteComment(adId, commentId, GetCurrentUserId());
                return Ok();
            }
            catch (InvalidOperationException ioe)
            {
                logger.LogDebug(ioe.Message + "\n" + ioe.StackTrace);
                return NotFound("Can't find ad, user or comment");
            }
            catch (ArgumentNullException ane)
            {
                logger.LogDebug(ane.Message + "\n" + ane.StackTrace);
                return NotFound("Can't find ad, user or comment");
            }
            catch (MethodAccessException mae)
            {
                logger.LogDebug(mae.Message + "\n" + mae.StackTrace);
                logger.LogDebug($"User {GetCurrentUserId()} has no rights to delete comment {commentId} in ad {adId}");
                return Forbid(JwtBearerDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme);
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
