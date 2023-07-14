using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
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
        private readonly IPricingService _pricingService;
        private readonly ISettingsManager _settingsManager;
        private readonly IItemService _itemService;
        private readonly IProductSearchService _productsSearchService;

        public ProductPriceDocumentBuilder(IPricingService pricingService, ISettingsManager settingsManager, IItemService itemService, IProductSearchService productsSearchService)
        {
            _pricingService = pricingService;
            _settingsManager = settingsManager;
            _itemService = itemService;
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

            // index min variation price
            foreach (var productId in documentIds)
            {
                var variationIds = new List<string>();

                var variationsSearchCriteria = GetVariationSearchCriteria(productId);
                var skipCount = 0;
                int totalCount;
                do
                {
                    variationsSearchCriteria.Skip = skipCount;
                    var productVariations = await _productsSearchService.SearchNoCloneAsync(variationsSearchCriteria);

                    variationIds.AddRange(productVariations.Results.Select(x => x.Id));

                    totalCount = productVariations.TotalCount;
                    skipCount += variationsSearchCriteria.Take;
                }
                while (skipCount < totalCount);

                // main product prices are already loaded above
                var variationPrices = prices.Where(x => x.ProductId == productId).ToList();

                // load all variations prices
                if (variationIds.Any())
                {
                    var bucket = (IList<Price>)new List<Price>(); //to IEnum

                    // check if variation prices are already loaded
                    if (variationIds.All(x => documentIds.Contains(x)))
                    {
                        bucket = prices.Where(x => variationIds.Contains(x.ProductId)).ToList();
                    }
                    else
                    {
                        bucket = await GetProductPrices(variationIds);
                    }

                    variationPrices.AddRange(bucket);
                }

                var document = result.FirstOrDefault(x => x.Id == productId);
                AddMinVariationPrice(document, variationPrices);
            }

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

            return (await _pricingService.EvaluateProductPricesAsync(evalContext)).ToList();
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

        private ProductSearchCriteria GetVariationSearchCriteria(string productId)
        {
            var criteria = AbstractTypeFactory<ProductSearchCriteria>.TryCreateInstance();

            criteria.MainProductId = productId;
            criteria.ResponseGroup = ItemResponseGroup.ItemInfo.ToString();
            criteria.Take = 50;

            return criteria;
        }

        private static void AddMinVariationPrice(IndexDocument document, IList<Price> prices)
        {
            if (document == null || prices.IsNullOrEmpty())
            {
                return;
            }

            var minPricesByCurrency = new List<IndexedPrice>();

            foreach (var group in prices.GroupBy(x => x.Currency))
            {
                var price = group.MinBy(x => x.EffectiveValue);
                var newPrice = new IndexedPrice
                {
                    Currency = price.Currency,
                    Value = price.EffectiveValue,
                };
                minPricesByCurrency.Add(newPrice);
            }

            document.Add(new IndexDocumentField("__minVariationPrice", minPricesByCurrency) { ValueType = IndexDocumentFieldValueType.Complex, IsRetrievable = false, IsFilterable = false, IsCollection = false });
        }
    }
}
