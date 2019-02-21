using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Data.Model;
using coreModel = VirtoCommerce.Domain.Pricing.Model;

namespace VirtoCommerce.PricingModule.Data.Extensions
{
    public static class IQueryableExtension
    {
        public static IQueryable<PricelistAssignmentEntity> BuildSearchQuery(this IQueryable<PricelistAssignmentEntity> query, PricelistAssignmentsSearchCriteria criteria)
        {
            query = GetQueryWithoutSkipTake(query, criteria);
            query = query.Skip(criteria.Skip).Take(criteria.Take);

            return query;
        }

        public static IQueryable<PricelistAssignmentEntity> BuildSearchQuery(this IQueryable<PricelistAssignmentEntity> query, PricelistAssignmentsSearchCriteria criteria, out int count)
        {
            query = GetQueryWithoutSkipTake(query, criteria);

            count = query.Count();

            query = query.Skip(criteria.Skip).Take(criteria.Take);

            return query;
        }

        private static IQueryable<PricelistAssignmentEntity> GetQueryWithoutSkipTake(this IQueryable<PricelistAssignmentEntity> query, PricelistAssignmentsSearchCriteria criteria)
        {
            if (!criteria.PriceListIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.PriceListIds.Contains(x.PricelistId));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Description.Contains(criteria.Keyword));
            }

            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[] { new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<coreModel.PricelistAssignment>(x => x.Priority) } };
            }

            query = query.OrderBySortInfos(sortInfos);

            return query;
        }
    }
}
