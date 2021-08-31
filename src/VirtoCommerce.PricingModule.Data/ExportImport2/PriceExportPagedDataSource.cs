using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PriceExportPagedDataSource : ExportPagedDataSource<PriceExportDataQuery, PricesSearchCriteria>
    {
        private readonly ICrudService<Price> _priceService;
        private readonly ISearchService<PricesSearchCriteria, PriceSearchResult, Price> _priceSearchService;
        private readonly ICrudService<Pricelist> _pricelistService;

        private readonly IItemService _itemService;
        private readonly PriceExportDataQuery _dataQuery;

        public PriceExportPagedDataSource(
            IPriceSearchService priceSearchService,
            IPriceService priceService,
            IPricelistService pricelistService,
            IItemService itemService,
            PriceExportDataQuery dataQuery) : base(dataQuery)
        {
            _priceSearchService = (ISearchService<PricesSearchCriteria, PriceSearchResult, Price>)priceSearchService;
            _priceService = (ICrudService <Price>)priceService;
            _pricelistService = (ICrudService<Pricelist>)pricelistService;
            _itemService = itemService;
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
                result = _priceService.GetByIdsAsync(searchCriteria.ObjectIds.AsEnumerable()).GetAwaiter().GetResult().ToArray();
                totalCount = result.Length;
            }
            else
            {
                var priceSearchResult = _priceSearchService.SearchAsync(searchCriteria).GetAwaiter().GetResult();
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
            var products = _itemService.GetByIdsAsync(productIds, ItemResponseGroup.ItemInfo.ToString()).GetAwaiter().GetResult();
            var pricelists = _pricelistService.GetByIdsAsync(pricelistIds).GetAwaiter().GetResult();

            foreach (var kvp in viewableMap)
            {
                var model = kvp.Key;
                var viewableEntity = kvp.Value;
                var product = products.FirstOrDefault(x => x.Id == model.ProductId);
                var pricelist = pricelists.FirstOrDefault(x => x.Id == model.PricelistId);

                viewableEntity.Code = product?.Code;
                viewableEntity.ImageUrl = product?.ImgSrc;
                viewableEntity.Name = product?.Name;
                viewableEntity.ProductName = product?.Name;
                viewableEntity.Parent = pricelist?.Name;
                viewableEntity.PricelistName = pricelist?.Name;
            }
        }
    }
}
