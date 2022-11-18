using System;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services.Search.Basic;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricelistSearchService : BasicPricelistSearchService<PricelistSearchResult, Pricelist, PricelistEntity>
    {
        public PricelistSearchService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
            ICrudService<Pricelist> pricelistService)
           : base(repositoryFactory, platformMemoryCache, pricelistService)
        {
        }
    }
}
