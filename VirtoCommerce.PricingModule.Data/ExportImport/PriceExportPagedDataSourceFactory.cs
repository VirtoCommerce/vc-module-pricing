using System;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PriceExportPagedDataSourceFactory : IPagedDataSourceFactory
    {
        private readonly IPricingSearchService _searchService;
        private readonly IPricingService _pricingService;
        private readonly IItemService _itemService;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public PriceExportPagedDataSourceFactory(IPricingSearchService searchService,
            IPricingService pricingService,
            IItemService itemService,
            IBlobUrlResolver blobUrlResolver)
        {
            _searchService = searchService;
            _pricingService = pricingService;
            _itemService = itemService;
            _blobUrlResolver = blobUrlResolver;
        }

        public IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            var priceExportDataQuery = dataQuery as PriceExportDataQuery ?? throw new InvalidCastException($"Cannot cast dataQuery to type {typeof(PriceExportDataQuery)}");

            return new PriceExportPagedDataSource(_searchService, _pricingService, _itemService, _blobUrlResolver, priceExportDataQuery);
        }
    }
}
