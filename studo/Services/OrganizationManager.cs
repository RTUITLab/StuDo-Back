using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using studo.Data;
using studo.Models;
using studo.Models.Requests.Organization;
using studo.Services.Configure;
using studo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Services
{
    public class OrganizationManager : IOrganizationManager
    {
        private readonly DatabaseContext dbContext;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;

        private IQueryable<OrganizationRight> OrganizationRights => dbContext.OrganizationRights;

        public IQueryable<Organization> Organizations => dbContext.Organizations;

        public OrganizationManager(DatabaseContext dbContext, IMapper mapper,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        public async Task<IQueryable<Organization>> AddAsync(OrganizationCreateRequest organizationCreateRequest, User creator)
        {
            if (creator == null)
                return null;

            var newOrg = mapper.Map<Organization>(organizationCreateRequest);
            newOrg.Users = new List<UserRightsInOrganization>();

            foreach (var right in OrganizationRights)
                newOrg.Users.Add(new UserRightsInOrganization
                {
                    UserId = creator.Id,
                    User = creator,
                    OrganizationRightId = right.Id,
                    UserOrganizationRight = right,
                    OrganizationId = newOrg.Id,
                    Organization = newOrg
                });
            
            newOrg.Ads = new List<Ad>();

            await dbContext.Organizations.AddAsync(newOrg);
            await dbContext.SaveChangesAsync();

            return dbContext.Organizations
                .Include(org => org.Users)
                .Include(org => org.Ads)
                .Where(org => org.Id == newOrg.Id);
        }

        public Task<IQueryable<Organization>> EditAsync(OrganizationEditRequest organizationEditRequest)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid organizationId)
        {
            throw new NotImplementedException();
        }
    }
}
