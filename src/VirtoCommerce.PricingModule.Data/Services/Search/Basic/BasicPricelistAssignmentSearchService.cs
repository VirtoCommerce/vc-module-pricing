using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;


namespace VirtoCommerce.PricingModule.Data.Services.Search.Basic
{
    public class BasicPricelistAssignmentSearchService<TResult, TModel, TEntity> : SearchService<PricelistAssignmentsSearchCriteria, TResult, TModel, TEntity> where TModel : PricelistAssignment where TEntity : PricelistAssignmentEntity, Platform.Core.Domain.IDataEntity<TEntity, TModel> where TResult : GenericSearchResult<TModel>
    {
        public BasicPricelistAssignmentSearchService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
            ICrudService<TModel> pricelistAssignmentService)
           : base(repositoryFactory, platformMemoryCache, pricelistAssignmentService)
        {
        }


        protected override IQueryable<TEntity> BuildQuery(IRepository repository, PricelistAssignmentsSearchCriteria criteria)
        {
            var query = (IQueryable<TEntity>) ((IPricingRepository)repository).PricelistAssignments;

            if (!criteria.PriceListIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.PriceListIds.Contains(x.PricelistId));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Description.Contains(criteria.Keyword));
            }

            if (!criteria.StoreIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.StoreIds.Contains(x.StoreId));
            }

            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.CatalogId));
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(PricelistAssignmentsSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = nameof(PricelistAssignment.Priority)
                    }
                };
            }
            return sortInfos;
        }
    }
}
