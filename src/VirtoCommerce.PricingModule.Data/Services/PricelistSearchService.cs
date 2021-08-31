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
    public class PricelistSearchService : SearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist, PricelistEntity>, IPricelistSearchService
    {

        public PricelistSearchService(Func<IPricelistRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
            IPricelistService pricelistService)
           : base(repositoryFactory, platformMemoryCache, (ICrudService<Pricelist>)pricelistService)
        {
        }
       
        protected override IQueryable<PricelistEntity> BuildQuery(IRepository repository, PricelistSearchCriteria criteria)
        {
            var query = ((IPricelistRepository)repository).Pricelists;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Description.Contains(criteria.Keyword));
            }

            if (!criteria.Currencies.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Currencies.Contains(x.Currency));
            }

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(PricelistSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = nameof(PricelistEntity.Name)
                    }
                };
            }
            return sortInfos;
        }
    }
}
