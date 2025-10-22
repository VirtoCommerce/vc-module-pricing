using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using VirtoCommerce.PricingModule2.Web.Extensions;

namespace VirtoCommerce.PricingModule2.Web.Services
{
    public class PriceSearchService2(
        Func<IPricingRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IPriceService crudService,
        IOptions<CrudOptions> crudOptions,
        IProductIndexedSearchService productIndexedSearchService,
        ISettingsManager settingsManager
    )
        : PriceSearchService(repositoryFactory, platformMemoryCache, crudService, crudOptions,
            productIndexedSearchService)
    {
        private readonly decimal _recommendedPricePercent = settingsManager.GetValue<decimal>(ModuleConstants.Settings.General.RecommendedPricePercent);

        public override async Task<PriceSearchResult> SearchAsync(PricesSearchCriteria criteria, bool clone = true)
        {
            var searchResult = await base.SearchAsync(criteria, clone);

            if (searchResult?.Results == null)
            {
                return searchResult;
            }

            searchResult.Results = searchResult.Results.FillRecommendedPrice(_recommendedPricePercent);

            return searchResult;
        }
    }
}
