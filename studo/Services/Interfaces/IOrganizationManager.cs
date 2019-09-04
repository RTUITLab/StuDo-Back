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
        Task<IQueryable<Organization>> AddAsync(OrganizationCreateRequest organizationCreateRequest, Guid creatorId);
        Task<IQueryable<Organization>> EditAsync(OrganizationEditRequest organizationEditRequest, Guid userId);
        Task DeleteAsync(Guid organizationId, Guid userId);
        Task AttachToRightAsync(AttachDetachRightRequest attachDetachRightRequest, Guid userId);
        Task DetachFromRightAsync(AttachDetachRightRequest attachDetachRightRequest, Guid userId);
    }
}
