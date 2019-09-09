using System;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PricingExportPagedDataSourceFactory : IPricingExportPagedDataSourceFactory
    {
        private readonly IPricingSearchService _searchService;
        private readonly IPricingService _pricingService;
        private readonly IItemService _itemService;
        private readonly ICatalogService _catalogService;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public PricingExportPagedDataSourceFactory(
            IPricingSearchService searchService
            , IPricingService pricingService
            , IItemService itemService
            , ICatalogService catalogService
            , IBlobUrlResolver blobUrlResolver)
        {
            _searchService = searchService;
            _pricingService = pricingService;
            _itemService = itemService;
            _catalogService = catalogService;
            _blobUrlResolver = blobUrlResolver;
        }

        public virtual IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            IPagedDataSource result = null;

            if (dataQuery is PriceExportDataQuery priceExportDataQuery)
            {
                result = new PriceExportPagedDataSource(_searchService, _pricingService, _itemService, _blobUrlResolver, priceExportDataQuery);
            }
            else if (dataQuery is PricelistAssignmentExportDataQuery pricelistAssignmentExportDataQuery)
            {
                result = new PricelistAssignmentExportPagedDataSource(_searchService, _pricingService, _catalogService, pricelistAssignmentExportDataQuery);
            }
            else if (dataQuery is PricelistExportDataQuery pricelistExportDataQuery)
            {
                result = new PricelistExportPagedDataSource(_searchService, _pricingService, pricelistExportDataQuery);
            }

            return result ?? throw new ArgumentException($"Unsupported export query type: {dataQuery.GetType().Name}");
        }
    }
}
