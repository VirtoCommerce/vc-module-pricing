using System;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services.Search.Basic;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PriceSearchService : BasicPriceSearchService<PriceSearchResult, Price, PriceEntity>
    {
        public PriceSearchService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
            ICrudService<Price> priceService, IProductIndexedSearchService productIndexedSearchService)
           : base(repositoryFactory, platformMemoryCache, priceService, productIndexedSearchService)
        {
        }
    }
}
