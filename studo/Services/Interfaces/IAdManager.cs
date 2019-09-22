using studo.Models;
using studo.Models.Requests.Ads;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Services.Interfaces
{
    public interface IAdManager
    {
        IQueryable<Ad> Ads { get; }
        Task<IQueryable<Ad>> AddAsync(AdCreateRequest adCreateRequest, Guid userId);
        Task<IQueryable<Ad>> EditAsync(AdEditRequest adEditRequest, Guid userId);
        Task DeleteAsync(Guid adId, Guid userId);
        Task AddToBookmarks(Guid adId, Guid userId);
        Task RemoveFromBookmarks(Guid adId, Guid userId);
        Task AddComment(Guid adId, Guid userId, AdCommentRequest adCommentRequest);
        Task DeleteComment(Guid commentId);
    }
}
