using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.Search
{
    public class ProductPriceDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly IPricingService _pricingService;
        private readonly ISettingsManager _settingsManager;

        public ProductPriceDocumentBuilder(IPricingService pricingService, ISettingsManager settingsManager)
        {
            _pricingService = pricingService;
            _settingsManager = settingsManager;
        }

        public virtual async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var prices = await GetProductPrices(documentIds);

            IList<IndexDocument> result = prices
                .GroupBy(p => p.ProductId)
                .Select(g => CreateDocument(g.Key, g.ToArray()))
                .ToArray();

            return result;
        }

        protected virtual IndexDocument CreateDocument(string productId, IList<Price> prices)
        {
            var document = new IndexDocument(productId);

            if (prices != null)
            {
                foreach (var price in prices)
                {
                    document.Add(new IndexDocumentField($"price_{price.Currency}_{price.PricelistId}".ToLowerInvariant(), price.EffectiveValue) { IsRetrievable = true, IsFilterable = true });
                }

                IndexPrice(document, prices);
                IndexPriceByCurrency(document, prices);

                document.Add(new IndexDocumentField("is", prices.Any(x => x.Sale > 0) ? "sale" : "nosale") { IsRetrievable = true, IsFilterable = true, IsCollection = true });
            }

            document.Add(new IndexDocumentField("is", prices?.Count > 0 ? "priced" : "unpriced") { IsRetrievable = true, IsFilterable = true, IsCollection = true });

            return document;
        }

        protected virtual async Task<IList<Price>> GetProductPrices(IList<string> productIds)
        {
            var evalContext = AbstractTypeFactory<PriceEvaluationContext>.TryCreateInstance();
            evalContext.ProductIds = productIds.ToArray();
            evalContext.CertainDate = DateTime.UtcNow;
            evalContext.SkipAssignmentValidation = true;
            evalContext.ReturnAllMatchedPrices = true;

            return (await _pricingService.EvaluateProductPricesAsync(evalContext)).ToList();
        }

        protected virtual void IndexPrice(IndexDocument document, IList<Price> prices)
        {
            var maxPrice = prices.Max(x => x.EffectiveValue);
            document.Add(new IndexDocumentField("price", maxPrice) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
        }

        protected virtual void IndexPriceByCurrency(IndexDocument document, IList<Price> prices)
        {
            foreach (var group in prices.GroupBy(x => x.Currency))
            {
                var maxCurrencyPrice = group.Max(x => x.EffectiveValue);
                document.Add(new IndexDocumentField($"price_{group.Key}".ToLowerInvariant(), maxCurrencyPrice) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
            }
        }
    }
}
