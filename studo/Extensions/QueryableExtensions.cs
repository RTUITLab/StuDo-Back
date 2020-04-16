using AutoMapper;
using AutoMapper.QueryableExtensions;
using studo.Models;
using studo.Models.Helpers;
using System;
using System.Linq;

namespace studo.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> IfNotNull<T, TS> (
            this IQueryable<T> queryable,
            TS? checkObj,
            Func<IQueryable<T>, IQueryable<T>> ifNotNullQueryable) where TS : struct
            => If(queryable, checkObj.HasValue, ifNotNullQueryable);

        public static IQueryable<T> If<T>(
            this IQueryable<T> queryable,
            bool check,
            Func<IQueryable<T>, IQueryable<T>> ifTrueQueryable)
            => check ? ifTrueQueryable(queryable) : queryable;

        public static IQueryable<AdAndUserId> AttachCurrentUserId(this IQueryable<Ad> source, IConfigurationProvider configuration, Guid currentUserId)
            => source.ProjectTo<AdAndUserId>(configuration, new { currentUserId });
    }
}
