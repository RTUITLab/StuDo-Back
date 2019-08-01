using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> userManager;

        public IQueryable<Organization> Organizations => dbContext.Organizations;

        public OrganizationManager(DatabaseContext dbContext, IMapper mapper,
            UserManager<User> userManager)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        public Task<IQueryable<Organization>> AddAsync(OrganizationCreateRequest organizationCreateRequest)
        {
            throw new NotImplementedException();
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
