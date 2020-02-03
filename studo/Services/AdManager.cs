using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using studo.Data;
using studo.Models;
using studo.Models.Requests.Ads;
using studo.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Services
{
    public class AdManager : IAdManager
    {
        private readonly DatabaseContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly UserManager<User> userManager;

        public IQueryable<Ad> Ads => dbContext.Ads;

        public AdManager(DatabaseContext dbContext, IMapper mapper, ILogger<AdManager> logger,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
        }

        public async Task<IQueryable<Ad>> AddAsync(AdCreateRequest adCreateRequest, Guid userId)
        {
            var newAd = mapper.Map<Ad>(adCreateRequest);

            if (newAd.OrganizationId.HasValue)
            {
                // check if such organization exists
                bool exist = await dbContext.Organizations
                    .Where(org => org.Id == newAd.OrganizationId.Value)
                    .AnyAsync();

                if (!exist)
                    throw new ArgumentNullException();

                // check if user has rights to create ads in organization
                bool hasRight = await dbContext.Organizations
                    .Where(org => org.Id == newAd.OrganizationId.Value)
                    .SelectMany(org => org.Users)
                    .Where(u => u.UserId == userId && u.OrganizationId == newAd.OrganizationId.Value)
                    .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == Configure.OrganizationRights.CanEditAd.ToString());

                if (!hasRight)
                    throw new MethodAccessException();

                Organization creator = await dbContext.Organizations
                    .FirstOrDefaultAsync(org => org.Id == newAd.OrganizationId.Value);

                newAd.Organization = creator;
                logger.LogDebug($"Current user {userId} created ad '{newAd.Id}' in organization {newAd.OrganizationId.Value}");
            }
            else
            {
                User creator = await userManager.FindByIdAsync(userId.ToString())
                    ?? throw new ArgumentNullException();
                newAd.User = creator;
            }

            await dbContext.Ads.AddAsync(newAd);
            await dbContext.SaveChangesAsync();
            return dbContext.Ads
                .Where(ad => ad.Id == newAd.Id);
        }

        public async Task<IQueryable<Ad>> EditAsync(AdEditRequest adEditRequest, Guid userId)
        {
            Ad adToEdit = await Ads.FirstOrDefaultAsync(ad => ad.Id == adEditRequest.Id)
                ?? throw new ArgumentNullException();

            if (adToEdit.OrganizationId.HasValue)
            {
                bool hasRight = await dbContext.Organizations
                    .Where(org => org.Id == adToEdit.OrganizationId.Value)
                    .SelectMany(org => org.Users)
                    .Where(u => u.UserId == userId)
                    .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == Configure.OrganizationRights.CanEditAd.ToString());

                if (!hasRight)
                    throw new MethodAccessException();

                logger.LogDebug($"Current user {userId} edited ad {adToEdit.Id} in organization {adToEdit.OrganizationId.Value}");
            }
            else
            {
                if (adToEdit.UserId.Value != userId)
                    throw new MethodAccessException();
            }

            mapper.Map(adEditRequest, adToEdit);

            dbContext.Ads.Update(adToEdit);
            await dbContext.SaveChangesAsync();
            return dbContext.Ads
                .Where(ad => ad.Id == adToEdit.Id);
        }

        public async Task DeleteAsync(Guid adId, Guid userId)
        {
            Ad adToDelete = await Ads.FirstOrDefaultAsync(ad => ad.Id == adId)
                ?? throw new ArgumentNullException();

            if (adToDelete.OrganizationId.HasValue)
            {
                bool hasRight = await dbContext.Organizations
                    .Where(org => org.Id == adToDelete.OrganizationId.Value)
                    .SelectMany(org => org.Users)
                    .Where(u => u.UserId == userId)
                    .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == Configure.OrganizationRights.CanEditAd.ToString());

                if (!hasRight)
                    throw new MethodAccessException();

                logger.LogDebug($"Current user {userId} deleted ad {adToDelete.Id} in organization {adToDelete.OrganizationId.Value}");
            }
            else
            {
                if (adToDelete.UserId.Value != userId)
                    throw new MethodAccessException();
            }

            dbContext.Ads.Remove(adToDelete);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddToBookmarks(Guid adId, Guid userId)
        {
            var ad = await Ads
                .Where(adv => adv.Id == adId)
                .Include(adv => adv.Bookmarks)
                .SingleAsync()
                ?? throw new ArgumentNullException();

            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new ArgumentNullException();

            if (ad.Bookmarks.Any(b => b.Ad == ad && b.User == user))
                throw new ArgumentException();

            ad.Bookmarks.Add(new UserAd
            {
                UserId = user.Id,
                User = user,
                AdId = ad.Id,
                Ad = ad
            });

            dbContext.Ads.Update(ad);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveFromBookmarks(Guid adId, Guid userId)
        {
            var ad = await Ads
                .Where(adv => adv.Id == adId)
                .Include(adv => adv.Bookmarks)
                .SingleAsync()
                ?? throw new ArgumentNullException();

            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new ArgumentNullException();

            if (!ad.Bookmarks.Any(b => b.Ad == ad && b.User == user))
                throw new ArgumentException();

            ad.Bookmarks.Remove(
                ad.Bookmarks.Find(b => b.Ad == ad && b.User == user));

            dbContext.Ads.Update(ad);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddComment(Guid adId, Guid userId, AdCommentRequest adCommentRequest)
        {
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new ArgumentNullException();

            var ad = await Ads
                .Where(a => a.Id == adId)
                .Include(a => a.Comments)
                .SingleAsync()
                ?? throw new ArgumentNullException();

            ad.Comments.Add(new Comment
            {
                Text = adCommentRequest.Text,
                AuthorId = user.Id,
                Author = user,
                AdId = ad.Id,
                Ad = ad,
                CommentTime = DateTime.UtcNow
            });
            dbContext.Ads.Update(ad);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteComment(Guid adId, Guid commentId, Guid userId)
        {
            Ad adToEdit = await Ads
                .Where(ad => ad.Id == adId)
                .Include(ad => ad.Comments)
                .SingleAsync()
                ?? throw new ArgumentNullException();

            Comment commentToDelete = adToEdit.Comments
                .Find(com => com.Id == commentId)
                ?? throw new ArgumentNullException();

            // who can - author of comment
            // author of ad
            // person, with rights CanEditAd

            if (adToEdit.UserId.HasValue)
            {
                if (adToEdit.UserId.Value != userId && commentToDelete.AuthorId != userId)
                    throw new MethodAccessException();
            }
            else
            {
                bool hasRight = await dbContext.Organizations
                    .Where(org => org.Id == adToEdit.OrganizationId.Value)
                    .SelectMany(org => org.Users)
                    .Where(u => u.UserId == userId)
                    .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == Configure.OrganizationRights.CanEditAd.ToString());

                if (!hasRight && commentToDelete.AuthorId != userId)
                    throw new MethodAccessException();
            }

            adToEdit.Comments.Remove(commentToDelete);
            dbContext.Ads.Update(adToEdit);
            await dbContext.SaveChangesAsync();
        }
    }
}
