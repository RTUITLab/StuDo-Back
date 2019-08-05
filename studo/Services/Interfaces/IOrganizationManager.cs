using studo.Models;
using studo.Models.Requests.Organization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Services.Interfaces
{
    public interface IOrganizationManager
    {
        IQueryable<Organization> Organizations { get; }
        Task<IQueryable<Organization>> AddAsync(OrganizationCreateRequest organizationCreateRequest, User creator);
        Task<IQueryable<Organization>> EditAsync(OrganizationEditRequest organizationEditRequest);
        Task DeleteAsync(Guid organizationId);
    }
}
