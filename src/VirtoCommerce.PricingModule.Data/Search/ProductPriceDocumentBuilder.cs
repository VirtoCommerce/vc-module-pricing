using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.Search
{
    public class ProductPriceDocumentBuilder : IIndexDocumentBuilder
    {
        private const int _batchSize = 50;

        private readonly IPricingEvaluatorService _pricingEvaluatorService;
        private readonly ISettingsManager _settingsManager;
        private readonly IProductSearchService _productsSearchService;

        public ProductPriceDocumentBuilder(IPricingEvaluatorService pricingEvaluatorService, ISettingsManager settingsManager, IProductSearchService productsSearchService)
        {
            _pricingEvaluatorService = pricingEvaluatorService;
            _settingsManager = settingsManager;
            _productsSearchService = productsSearchService;
        }

        public virtual async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var prices = await GetProductPrices(documentIds);

            var useMaxIndexationPrice = UseMaxIndexationPrice();

            IList<IndexDocument> result = prices
                .GroupBy(p => p.ProductId)
                .Select(g => CreateDocument(g.Key, g.ToArray(), useMaxIndexationPrice))
                .ToArray();

            await AddMinPrices(result, documentIds, prices);

            return result;
        }

        protected virtual IndexDocument CreateDocument(string productId, IList<Price> prices, bool useMaxIndexationPrice)
        {
            var document = new IndexDocument(productId);

            if (prices != null)
            {
                foreach (var price in prices)
                {
                    document.Add(new IndexDocumentField($"price_{price.Currency}_{price.PricelistId}".ToLowerInvariant(), price.EffectiveValue) { IsRetrievable = true, IsFilterable = true });
                }

                IndexPrice(document, prices, useMaxIndexationPrice);
                IndexPriceByCurrency(document, prices, useMaxIndexationPrice);

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

            return (await _pricingEvaluatorService.EvaluateProductPricesAsync(evalContext)).ToList();
        }

        protected virtual void IndexPrice(IndexDocument document, IList<Price> prices, bool useMaxIndexationPrice)
        {
            var price = useMaxIndexationPrice
                ? prices.Max(x => x.EffectiveValue)
                : prices.Min(x => x.EffectiveValue);
            document.Add(new IndexDocumentField("price", price) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
        }

        protected virtual void IndexPriceByCurrency(IndexDocument document, IList<Price> prices, bool useMaxIndexationPrice)
        {
            foreach (var group in prices.GroupBy(x => x.Currency))
            {
                var currencyPrice = useMaxIndexationPrice
                    ? group.Max(x => x.EffectiveValue)
                    : group.Min(x => x.EffectiveValue);
                document.Add(new IndexDocumentField($"price_{group.Key}".ToLowerInvariant(), currencyPrice) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
            }
        }

        private bool UseMaxIndexationPrice()
        {
            var value = _settingsManager.GetValue(ModuleConstants.Settings.General.PriceIndexingValue.Name, ModuleConstants.Settings.General.PriceIndexingValue.DefaultValue as string);
            return value.EqualsInvariant(ModuleConstants.Settings.General.PriceIndexingValueMax);
        }

        private async Task AddMinPrices(IList<IndexDocument> documents, IList<string> documentIds, IList<Price> prices)
        {
            foreach (var document in documents)
            {
                var productId = document.Id;

                // Main product prices are already loaded above
                var productPrices = prices.Where(x => x.ProductId == productId).ToList();

                // Load all variation prices
                var variationIds = await GetVariationIds(productId);

                if (variationIds.Any())
                {
                    var variationPrices = prices.Where(x => variationIds.Contains(x.ProductId)).ToList();
                    productPrices.AddRange(variationPrices);

                    var missingVariationIds = variationIds.Except(documentIds).ToList();
                    if (missingVariationIds.Any())
                    {
                        var missingPrices = await GetProductPrices(missingVariationIds);
                        productPrices.AddRange(missingPrices);
                    }
                }

                if (productPrices.Any())
                {
                    AddMinPrice(document, productPrices);
                }
            }
        }

        private async Task<List<string>> GetVariationIds(string productId)
        {
            var variationIds = new List<string>();

            var searchCriteria = AbstractTypeFactory<ProductSearchCriteria>.TryCreateInstance();
            searchCriteria.MainProductId = productId;
            searchCriteria.ResponseGroup = ItemResponseGroup.ItemInfo.ToString();
            searchCriteria.Take = _batchSize;

            await foreach (var searchResult in _productsSearchService.SearchBatches(searchCriteria))
            {
                variationIds.AddRange(searchResult.Results.Select(x => x.Id));
            }

            return variationIds;
        }

        private static void AddMinPrice(IndexDocument document, IList<Price> prices)
        {
            // Needs a list of objects - workaround for a bug in ES provider
            var minPricesByCurrency = prices
                .GroupBy(x => x.Currency)
                .Select(group => group.MinBy(x => x.EffectiveValue))
                .Select(price => new IndexedPrice
                {
                    Currency = price.Currency,
                    Value = price.EffectiveValue,
                })
                .ToList<object>();

            document.Add(new IndexDocumentField("__minVariationPrice", minPricesByCurrency) { ValueType = IndexDocumentFieldValueType.Complex, IsRetrievable = false, IsFilterable = false, IsCollection = false });
        }
    }
}
