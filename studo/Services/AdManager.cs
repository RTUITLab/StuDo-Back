using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        private readonly UserManager<User> userManager;

        public IQueryable<Ad> Ads => dbContext.Ads;

        public AdManager(DatabaseContext dbContext, IMapper mapper,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        public async Task<IQueryable<Ad>> AddAsync(AdCreateRequest adCreateRequest)
        {
            var newAd = mapper.Map<Ad>(adCreateRequest);

            if (newAd.OrganizationId.HasValue)
            {
                var creator = await dbContext.Organizations.FindAsync(newAd.OrganizationId.Value.ToString());
                newAd.Organization = creator;
            }
            else
            {
                var creator = await userManager.FindByIdAsync(newAd.UserId.Value.ToString());
                newAd.User = creator;
            }

            await dbContext.Ads.AddAsync(newAd);
            await dbContext.SaveChangesAsync();
            return dbContext.Ads
                .Include(ad => ad.User)
                .Include(ad => ad.Organization)
                .Where(ad => ad.Id == newAd.Id);
        }

        // TODO: check if user in organization can edit ads
        // or make separate controllers for user's ads and org's ads

        public async Task<IQueryable<Ad>> EditAsync(AdEditRequest adEditRequest)
        {
            var adToEdit = await dbContext.Ads.FindAsync(adEditRequest.Id);
            if (adToEdit == null)
                return null;

            mapper.Map(adEditRequest, adToEdit);

            await dbContext.SaveChangesAsync();
            return dbContext.Ads
                .Include(ad => ad.User)
                .Include(ad => ad.Organization)
                .Where(ad => ad.Id == adToEdit.Id);
        }

        public async Task DeleteAsync(Guid adId)
        {
            var adToDelete = await dbContext.Ads.FindAsync(adId);
            dbContext.Ads.Remove(adToDelete);
            await dbContext.SaveChangesAsync();
        }
    }
}
