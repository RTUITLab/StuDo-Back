using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using studo.Data;
using studo.Models;
using studo.Models.Requests.Organization;
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
        private readonly ILogger<OrganizationManager> logger;

        private IQueryable<OrganizationRight> OrganizationRights => dbContext.OrganizationRights;

        public IQueryable<Organization> Organizations => dbContext.Organizations;

        public OrganizationManager(DatabaseContext dbContext, IMapper mapper,
            ILogger<OrganizationManager> logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<IQueryable<Organization>> AddAsync(OrganizationCreateRequest organizationCreateRequest, User creator)
        {
            if (creator == null)
            {
                logger.LogDebug("Creator = null");
                return null;
            }

            var newOrg = mapper.Map<Organization>(organizationCreateRequest);
            newOrg.Users = new List<UserRightsInOrganization>();

            logger.LogDebug("Start adding creator to all roles");
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
            logger.LogDebug("End adding creator to all roles");

            newOrg.Ads = new List<Ad>();

            await dbContext.Organizations.AddAsync(newOrg);
            logger.LogDebug("Added new organization");

            await dbContext.SaveChangesAsync();
            logger.LogDebug("Save all changes");

            return dbContext.Organizations
                .Where(org => org.Id == newOrg.Id);
        }

        public async Task<IQueryable<Organization>> EditAsync(OrganizationEditRequest organizationEditRequest, Guid userId)
        {
            bool exist = await Organizations
                .Where(org => org.Id == organizationEditRequest.Id)
                .AnyAsync();

            if (!exist)
                throw new ArgumentException();

            bool hasRight = await Organizations
                .Where(org => org.Id == organizationEditRequest.Id)
                .SelectMany(org => org.Users)
                .Where(u => u.UserId == userId && u.OrganizationId == organizationEditRequest.Id)
                .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == Configure.OrganizationRights.CanEditOrganizationInformation.ToString());

            if (!hasRight)
                throw new MethodAccessException();

            Organization orgToEdit = mapper.Map<Organization>(organizationEditRequest);

            dbContext.Organizations.Update(orgToEdit);
            await dbContext.SaveChangesAsync();
            return Organizations
                .Where(org => org.Id == organizationEditRequest.Id);
        }

        public Task DeleteAsync(Guid organizationId)
        {
            throw new NotImplementedException();
        }
    }
}
