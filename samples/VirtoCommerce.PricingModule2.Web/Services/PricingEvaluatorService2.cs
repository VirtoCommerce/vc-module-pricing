using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using VirtoCommerce.PricingModule2.Web.Extensions;

namespace VirtoCommerce.PricingModule2.Web.Services;

public class PricingEvaluatorService2(
    ISettingsManager settingsManager,
    Func<IPricingRepository> repositoryFactory,
    IItemService productService,
    ILogger<PricingEvaluatorService> logger,
    IPlatformMemoryCache platformMemoryCache,
    IPricingPriorityFilterPolicy pricingPriorityFilterPolicy
)
    : PricingEvaluatorService(repositoryFactory: repositoryFactory,
        productService: productService,
        logger: logger,
        platformMemoryCache: platformMemoryCache,
        pricingPriorityFilterPolicy: pricingPriorityFilterPolicy)
{
    private readonly decimal _recommendedPricePercent = settingsManager.GetValue<decimal>(ModuleConstants.Settings.General.RecommendedPricePercent);

    public override async Task<IList<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext)
    {
        var prices = await base.EvaluateProductPricesAsync(evalContext);

        prices = prices.FillRecommendedPrice(_recommendedPricePercent);

        return prices;
    }
}
