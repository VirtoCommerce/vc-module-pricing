using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using VirtoCommerce.PricingModule2.Web.Models;

namespace VirtoCommerce.PricingModule2.Web.Services;

public class PricingEvaluatorService2 : PricingEvaluatorService
{
    private readonly ISettingsManager _settingsManager;
    private readonly Func<IPricingRepository> _repositoryFactory;

    public PricingEvaluatorService2(
        ISettingsManager settingsManager,
        Func<IPricingRepository> repositoryFactory,
        IItemService productService,
        ILogger<PricingEvaluatorService> logger,
        IPlatformMemoryCache platformMemoryCache,
        IPricingPriorityFilterPolicy pricingPriorityFilterPolicy)
        : base(
            settingsManager: settingsManager,
            repositoryFactory: repositoryFactory,
            productService: productService,
            logger: logger,
            platformMemoryCache: platformMemoryCache,
            pricingPriorityFilterPolicy: pricingPriorityFilterPolicy)
    {
        _settingsManager = settingsManager;
        _repositoryFactory = repositoryFactory;
    }

    public override async Task<IList<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext)
    {
        ArgumentNullException.ThrowIfNull(evalContext);

        if (evalContext.ProductIds == null)
        {
            throw new MissingFieldException("ProductIds");
        }

        var result = new List<Price>();

        IEnumerable<Price> prices;

        using (var repository = _repositoryFactory())
        {
            //Get a price range satisfying by passing context
            var query = (repository).Prices
                .Include(x => x.Pricelist).AsSingleQuery()
                .Where(x => evalContext.ProductIds.Contains(x.ProductId))
                .Where(x => evalContext.Quantity >= x.MinQuantity || evalContext.Quantity == 0);

            if (evalContext.PricelistIds.IsNullOrEmpty())
            {
                evalContext.Pricelists = evalContext.Pricelists.IsNullOrEmpty()
                    ? (await EvaluatePriceListsAsync(evalContext)).ToArray()
                    : evalContext.Pricelists;

                evalContext.PricelistIds = evalContext.Pricelists.Select(x => x.Id).ToArray();
            }

            query = query.Where(x => evalContext.PricelistIds.Contains(x.PricelistId));

            // Filter by date expiration
            // Always filter on date, so that we limit the results to process.
            var certainDate = evalContext.CertainDate ?? DateTime.UtcNow;
            query = query.Where(x => (x.StartDate == null || x.StartDate <= certainDate)
                && (x.EndDate == null || x.EndDate > certainDate));

            var queryResult = await query.AsNoTracking().ToListAsync();
            prices = queryResult.Select(x => x.ToModel(AbstractTypeFactory<Price>.TryCreateInstance()));

            prices = prices.Where(x => x is Price2 { RecommendedPrice: null }).Select(FillRecommendedPrice);
        }

        result.AddRange(await PostProcessPrices(evalContext, prices));

        return result;
    }

    private Price FillRecommendedPrice(Price price)
    {
        if (price == null)
        {
            return null;
        }

        if (price is not Price2 price2)
        {
            return price;
        }

        var recommendedPricePercent = _settingsManager.GetValue<decimal>(ModuleConstants.Settings.General.RecommendedPricePercent);

        if (price2.Sale != null)
        {
            price2.RecommendedPrice = price2.Sale.Value * recommendedPricePercent;
        }
        else
        {
            price2.RecommendedPrice = price2.List * recommendedPricePercent;
        }

        return price2;
    }
}
