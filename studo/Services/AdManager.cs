using AutoMapper;
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

        public IQueryable<Ad> Ads => dbContext.Ads;

        public AdManager(DatabaseContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public async Task<IQueryable<Ad>> AddAsync(AdCreateRequest adCreateRequest)
        {
            var newAd = mapper.Map<Ad>(adCreateRequest);
            await dbContext.Ads.AddAsync(newAd);
            await dbContext.SaveChangesAsync();
            return dbContext.Ads.Where(ad => ad.Id == newAd.Id);
        }

        public async Task<IQueryable<Ad>> EditAsync(AdEditRequest adEditRequest)
        {
            var adToEdit = await dbContext.Ads.FindAsync(adEditRequest.Id);
            if (adToEdit == null)
                return null;

            mapper.Map(adEditRequest, adToEdit);

            await dbContext.SaveChangesAsync();
            return dbContext.Ads.Where(ad => ad.Id == adToEdit.Id);
        }

        public async Task DeleteAsync(Guid adId)
        {
            var adToDelete = await dbContext.Ads.FindAsync(adId);
            dbContext.Ads.Remove(adToDelete);
            await dbContext.SaveChangesAsync();
        }
    }
}
