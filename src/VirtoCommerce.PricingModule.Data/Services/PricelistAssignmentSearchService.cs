using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricelistAssignmentSearchService : SearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment, PricelistAssignmentEntity>
                                                    , IPricelistAssignmentSearchService
    {

        public PricelistAssignmentSearchService(Func<IPricelistAssignmentRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
            IPricelistAssignmentService pricelistAssignmentService)
           : base(repositoryFactory, platformMemoryCache, (ICrudService<PricelistAssignment>)pricelistAssignmentService)
        {
        }

        protected override IQueryable<PricelistAssignmentEntity> BuildQuery(IRepository repository, PricelistAssignmentsSearchCriteria criteria)
        {
            var query = ((IPricelistAssignmentRepository)repository).PricelistAssignments;

            if (!criteria.PriceListIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.PriceListIds.Contains(x.PricelistId));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Description.Contains(criteria.Keyword));
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
