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
    }
}
