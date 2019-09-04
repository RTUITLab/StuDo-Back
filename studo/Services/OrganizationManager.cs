using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<OrganizationManager> logger;
        private readonly UserManager<User> userManager;

        private IQueryable<OrganizationRight> OrganizationRights => dbContext.OrganizationRights;

        public IQueryable<Organization> Organizations => dbContext.Organizations;

        public OrganizationManager(DatabaseContext dbContext, IMapper mapper,
            ILogger<OrganizationManager> logger, UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
            this.userManager = userManager;
        }

        public async Task<IQueryable<Organization>> AddAsync(OrganizationCreateRequest organizationCreateRequest, Guid creatorId)
        {
            bool exist = await Organizations
                .Where(org => org.Name == organizationCreateRequest.Name)
                .AnyAsync();

            if (exist)
                throw new ArgumentException();

            var creator = await userManager.FindByIdAsync(creatorId.ToString())
                ?? throw new ArgumentNullException("Can't find creator");

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
                throw new ArgumentNullException();

            exist = await Organizations
                .Where(org => org.Name == organizationEditRequest.Name)
                .AnyAsync();

            if (exist)
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

        public async Task DeleteAsync(Guid organizationId, Guid userId)
        {
            Organization orgToDelete = await Organizations
                .Where(org => org.Id == organizationId)
                .Include(org => org.Ads)
                .SingleAsync()
                ?? throw new ArgumentNullException();

            bool hasRight = await Organizations
                .Where(org => org.Id == organizationId)
                .SelectMany(org => org.Users)
                .Where(u => u.UserId == userId && u.OrganizationId == organizationId)
                .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == Configure.OrganizationRights.CanDeleteOrganization.ToString());

            if (!hasRight)
                throw new MethodAccessException();

            dbContext.Ads.RemoveRange(orgToDelete.Ads);
            dbContext.Organizations.Remove(orgToDelete);
            await dbContext.SaveChangesAsync();
        }

        public async Task AttachToRightAsync(AttachDetachRightRequest attachDetachRightRequest, Guid userId)
        {
            bool exist = await dbContext.OrganizationRights
                .AnyAsync(or => or.RightName == attachDetachRightRequest.Right);

            if (!exist)
            {
                logger.LogDebug($"Right {attachDetachRightRequest.Right} doesn't exist");
                throw new ArgumentNullException();
            }

            if (attachDetachRightRequest.Right == Configure.OrganizationRights.CanDeleteOrganization.ToString())
            {
                logger.LogDebug($"Right {attachDetachRightRequest.Right} can't be attached");
                throw new ArgumentException();
            }

            exist = await Organizations
                .AnyAsync(org => org.Id == attachDetachRightRequest.OrganizationId);

            if (!exist)
            {
                logger.LogDebug($"Organization {attachDetachRightRequest.OrganizationId} doesn't exist");
                throw new ArgumentNullException();
            }

            exist = await userManager.Users
                .AnyAsync(u => u.Id == attachDetachRightRequest.UserId);

            if (!exist)
            {
                logger.LogDebug($"User {attachDetachRightRequest.UserId} doesn't exist");
                throw new ArgumentNullException();
            }

            exist = await userManager.Users
                .AnyAsync(u => u.Id == userId);

            if (!exist)
            {
                logger.LogDebug($"Current user {userId} doesn't exist");
                throw new ArgumentNullException();
            }

            bool hasRight = await Organizations
                .Where(org => org.Id == attachDetachRightRequest.OrganizationId)
                .SelectMany(org => org.Users)
                .Where(u => u.UserId == userId && u.OrganizationId == attachDetachRightRequest.OrganizationId)
                .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == Configure.OrganizationRights.CanEditRights.ToString());

            if (!hasRight)
            {
                logger.LogDebug($"Current user {userId} doesn't have rights to edit rights in organization {attachDetachRightRequest.OrganizationId}");
                throw new MethodAccessException();
            }

            exist = await Organizations
                .Where(org => org.Id == attachDetachRightRequest.OrganizationId)
                .SelectMany(org => org.Users)
                .Where(u => u.UserId == attachDetachRightRequest.UserId && u.OrganizationId == attachDetachRightRequest.OrganizationId)
                .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == attachDetachRightRequest.Right);

            if (exist)
            {
                logger.LogDebug($"User {attachDetachRightRequest.UserId} already has right {attachDetachRightRequest.Right}");
                throw new MemberAccessException();
            }

            Organization organization = await Organizations
                .Where(org => org.Id == attachDetachRightRequest.OrganizationId)
                .Include(org => org.Users)
                .SingleAsync();

            User user = await userManager.FindByIdAsync(attachDetachRightRequest.UserId.ToString());

            OrganizationRight organizationRight = await dbContext.OrganizationRights.FirstOrDefaultAsync(or => or.RightName == attachDetachRightRequest.Right);

            organization.Users.Add(new UserRightsInOrganization
            {
                OrganizationId = organization.Id,
                Organization = organization,
                UserId = user.Id,
                User = user,
                OrganizationRightId = organizationRight.Id,
                UserOrganizationRight = organizationRight
            });

            dbContext.Organizations.Update(organization);
            await dbContext.SaveChangesAsync();
            logger.LogDebug($"Current user {userId} attached user {attachDetachRightRequest.UserId} to right {attachDetachRightRequest.Right} in organizaiton {attachDetachRightRequest.OrganizationId}");
        }

        public async Task DetachFromRightAsync(AttachDetachRightRequest attachDetachRightRequest, Guid userId)
        {
            bool exist = await dbContext.OrganizationRights
                .AnyAsync(or => or.RightName == attachDetachRightRequest.Right);

            if (!exist)
            {
                logger.LogDebug($"Right {attachDetachRightRequest.Right} doesn't exist");
                throw new ArgumentNullException();
            }

            if (attachDetachRightRequest.Right == Configure.OrganizationRights.CanDeleteOrganization.ToString())
            {
                logger.LogDebug($"Can't detach from right {attachDetachRightRequest.Right}");
                throw new ArgumentException();
            }

            exist = await Organizations
                .AnyAsync(org => org.Id == attachDetachRightRequest.OrganizationId);

            if (!exist)
            {
                logger.LogDebug($"Organization {attachDetachRightRequest.OrganizationId} doesn't exist");
                throw new ArgumentNullException();
            }

            exist = await userManager.Users
                .AnyAsync(u => u.Id == attachDetachRightRequest.UserId);

            if (!exist)
            {
                logger.LogDebug($"Current user {userId} doesn't exist");
                throw new ArgumentNullException();
            }

            bool hasRight = await Organizations
                .Where(org => org.Id == attachDetachRightRequest.OrganizationId)
                .SelectMany(org => org.Users)
                .Where(u => u.UserId == userId && u.OrganizationId == attachDetachRightRequest.OrganizationId)
                .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == Configure.OrganizationRights.CanEditRights.ToString());

            if (!hasRight)
            {
                logger.LogDebug($"Current user {userId} doesn't have rights to edit rights in organization {attachDetachRightRequest.OrganizationId}");
                throw new MethodAccessException();
            }

            exist = await Organizations
                .Where(org => org.Id == attachDetachRightRequest.OrganizationId)
                .SelectMany(org => org.Users)
                .Where(u => u.UserId == attachDetachRightRequest.UserId && u.OrganizationId == attachDetachRightRequest.OrganizationId)
                .AnyAsync(userorgright => userorgright.UserOrganizationRight.RightName == attachDetachRightRequest.Right);

            if (!exist)
            {
                logger.LogDebug($"User {attachDetachRightRequest.UserId} doesn't have right {attachDetachRightRequest.Right}");
                throw new MemberAccessException();
            }

            Organization organization = await Organizations
                .Where(org => org.Id == attachDetachRightRequest.OrganizationId)
                .Include(org => org.Users)
                .SingleAsync();

            User user = await userManager.FindByIdAsync(attachDetachRightRequest.UserId.ToString());

            OrganizationRight organizationRight = await dbContext.OrganizationRights.FirstOrDefaultAsync(or => or.RightName == attachDetachRightRequest.Right);

            UserRightsInOrganization userRightsInOrganization = organization.Users.FirstOrDefault(userorgright =>
                userorgright.UserId == user.Id && userorgright.OrganizationId == organization.Id && userorgright.OrganizationRightId == organizationRight.Id);

            organization.Users.Remove(userRightsInOrganization);
            dbContext.Organizations.Update(organization);
            await dbContext.SaveChangesAsync();
            logger.LogDebug($"Current user {userId} detached user {attachDetachRightRequest.UserId} from right {attachDetachRightRequest.Right} in organizaiton {attachDetachRightRequest.OrganizationId}");
        }
    }
}
