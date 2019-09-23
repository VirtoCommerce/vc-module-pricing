using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PriceExportPagedDataSource : ExportPagedDataSource<PriceExportDataQuery, PricesSearchCriteria>
    {
        private readonly IPricingSearchService _searchService;
        private readonly IPricingService _pricingService;
        private readonly IItemService _itemService;
        private readonly PriceExportDataQuery _dataQuery;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public PriceExportPagedDataSource(
            IPricingSearchService searchService,
            IPricingService pricingService,
            IItemService itemService,
            IBlobUrlResolver blobUrlResolver,
            PriceExportDataQuery dataQuery) : base(dataQuery)
        {
            _searchService = searchService;
            _pricingService = pricingService;
            _itemService = itemService;
            _blobUrlResolver = blobUrlResolver;
            _dataQuery = dataQuery;
        }


        protected override PricesSearchCriteria BuildSearchCriteria(PriceExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.PriceListIds = _dataQuery.PriceListIds;
            result.ProductIds = _dataQuery.ProductIds;
            result.ModifiedSince = _dataQuery.ModifiedSince;
            result.Keyword = _dataQuery.Keyword;

            return result;
        }

        protected override ExportableSearchResult FetchData(PricesSearchCriteria searchCriteria)
        {
            Price[] result;
            int totalCount;

            if (searchCriteria.ObjectIds.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                result = _pricingService.GetPricesById(Enumerable.ToArray(searchCriteria.ObjectIds));
                totalCount = result.Length;
            }
            else
            {
                var priceSearchResult = _searchService.SearchPrices(searchCriteria);
                result = priceSearchResult.Results.ToArray();
                totalCount = priceSearchResult.TotalCount;
            }

            return new ExportableSearchResult()
            {
                Results = ToExportable(result).ToList(),
                TotalCount = totalCount,
            };
        }

        protected virtual IEnumerable<IExportable> ToExportable(IEnumerable<ICloneable> objects)
        {
            var models = objects.Cast<Price>();
            var viewableMap = models.ToDictionary(x => x, x => AbstractTypeFactory<ExportablePrice>.TryCreateInstance().FromModel(x));

            FillAdditionalProperties(viewableMap);

            var modelIds = models.Select(x => x.Id).ToList();

            return viewableMap.Values.OrderBy(x => modelIds.IndexOf(x.Id));
        }

        protected virtual void FillAdditionalProperties(Dictionary<Price, ExportablePrice> viewableMap)
        {
            var models = viewableMap.Keys;
            var productIds = models.Select(x => x.ProductId).Distinct().ToArray();
            var pricelistIds = models.Select(x => x.PricelistId).Distinct().ToArray();
            var products = _itemService.GetByIds(productIds, ItemResponseGroup.ItemInfo);
            var pricelists = _pricingService.GetPricelistsById(pricelistIds);

            foreach (var kvp in viewableMap)
            {
                var model = kvp.Key;
                var viewableEntity = kvp.Value;
                var product = products.FirstOrDefault(x => x.Id == model.ProductId);
                var pricelist = pricelists.FirstOrDefault(x => x.Id == model.PricelistId);
                var imageUrl = product?.Images?.FirstOrDefault()?.Url;

                viewableEntity.Code = product?.Code;
                viewableEntity.ImageUrl = imageUrl != null ? _blobUrlResolver.GetAbsoluteUrl(imageUrl) : null;
                viewableEntity.Name = product?.Name;
                viewableEntity.ProductName = product?.Name;
                viewableEntity.Parent = pricelist?.Name;
                viewableEntity.PricelistName = pricelist?.Name;
            }
        }
    }
}
