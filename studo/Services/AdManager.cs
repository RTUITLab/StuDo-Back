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
        private readonly IOrganizationManager organizationManager;

        public IQueryable<Ad> Ads => dbContext.Ads;

        public AdManager(DatabaseContext dbContext, IMapper mapper, ILogger<AdManager> logger,
            UserManager<User> userManager, IOrganizationManager organizationManager)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
            this.organizationManager = organizationManager;
        }

        public async Task<IQueryable<Ad>> AddAsync(AdCreateRequest adCreateRequest, Guid userId)
        {
            var newAd = mapper.Map<Ad>(adCreateRequest);

            if (newAd.OrganizationId.HasValue)
            {
                // check if such organization exists
                bool exist = await organizationManager.Organizations
                    .Where(org => org.Id == newAd.OrganizationId.Value)
                    .AnyAsync();

                if (!exist)
                    throw new ArgumentNullException();

                // check if user has rights to create ads in organization
                bool hasRight = await organizationManager.Organizations
                    .Where(org => org.Id == newAd.OrganizationId.Value)
                    .SelectMany(org => org.Users)
                    .Where(u => u.UserId == userId && u.OrganizationId == newAd.OrganizationId.Value)
                    .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == Configure.OrganizationRights.CanEditAd.ToString());

                if (!hasRight)
                    throw new MethodAccessException();

                Organization creator = await organizationManager.Organizations
                    .FirstOrDefaultAsync(org => org.Id == newAd.OrganizationId.Value);

                newAd.Organization = creator;
                logger.LogDebug($"Current user {userId} created ad '{adCreateRequest.Name}' in organization {newAd.OrganizationId.Value}");
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

        // TODO: check if user in organization can edit ads

        public async Task<IQueryable<Ad>> EditAsync(AdEditRequest adEditRequest, Guid userId)
        {
            Ad adToEdit = await Ads.FirstOrDefaultAsync(ad => ad.Id == adEditRequest.Id)
                ?? throw new ArgumentException();

            bool hasRight = await Ads
                .Where(ad => ad.Id == adEditRequest.Id)
                .AnyAsync(ad => ad.UserId.HasValue && ad.UserId.Value == userId);

            if (!hasRight)
                throw new MethodAccessException();

            mapper.Map(adEditRequest, adToEdit);

            dbContext.Ads.Update(adToEdit);
            await dbContext.SaveChangesAsync();
            return dbContext.Ads
                .Where(ad => ad.Id == adToEdit.Id);
        }

        public async Task DeleteAsync(Guid adId, Guid userId)
        {
            Ad adToDelete = await Ads.FirstOrDefaultAsync(ad => ad.Id == adId)
                ?? throw new ArgumentException();

            if (!adToDelete.UserId.HasValue || adToDelete.UserId.Value != userId)
                throw new MethodAccessException();

            dbContext.Ads.Remove(adToDelete);
            await dbContext.SaveChangesAsync();
        }
    }
}
