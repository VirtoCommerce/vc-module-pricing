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
using VirtoCommerce.PricingModule2.Web.Models;

namespace VirtoCommerce.PricingModule2.Web.Services
{
    public class PriceSearchService2 : PriceSearchService
    {
        private readonly ISettingsManager _settingsManager;

        public PriceSearchService2(
            Func<IPricingRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IPriceService crudService,
            IOptions<CrudOptions> crudOptions,
            IProductIndexedSearchService productIndexedSearchService,
            ISettingsManager settingsManager)
           : base(repositoryFactory, platformMemoryCache, crudService, crudOptions, productIndexedSearchService)
        {
            _settingsManager = settingsManager;
        }

        public override async Task<PriceSearchResult> SearchAsync(PricesSearchCriteria criteria, bool clone = true)
        {
            var searchResult = await base.SearchAsync(criteria, clone);

            if (searchResult?.Results != null)
            {
                var recommendedPricePercent = _settingsManager.GetValue<decimal>(ModuleConstants.Settings.General.RecommendedPricePercent);

                foreach (var price in searchResult.Results)
                {
                    if (price is Price2 price2 && price2.RecommendedPrice == null)
                    {
                        FillRecommendedPrice(price2, recommendedPricePercent);
                    }
                }
            }

            return searchResult;
        }

        private static void FillRecommendedPrice(Price2 price2, decimal recommendedPricePercent)
        {
            if (price2.Sale != null)
            {
                price2.RecommendedPrice = price2.Sale.Value * recommendedPricePercent;
            }
            else
            {
                price2.RecommendedPrice = price2.List * recommendedPricePercent;
            }
        }
    }
}
