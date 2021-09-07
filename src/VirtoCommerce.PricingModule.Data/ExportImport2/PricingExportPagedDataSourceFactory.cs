using System;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PricingExportPagedDataSourceFactory : IPricingExportPagedDataSourceFactory
    {
        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;

        private readonly IItemService _itemService;
        private readonly ICatalogService _catalogService;

        public PricingExportPagedDataSourceFactory(IPricingService pricingService
            , IPricingSearchService pricingSearchService
            , IItemService itemService
            , ICatalogService catalogService)
        {
            _pricingService = pricingService;
            _pricingSearchService = pricingSearchService;
            _itemService = itemService;
            _catalogService = catalogService;
        }

        public virtual IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            IPagedDataSource result = null;

            if (dataQuery is PriceExportDataQuery priceExportDataQuery)
            {
                result = new PriceExportPagedDataSource(_pricingService, _pricingSearchService,  _itemService, priceExportDataQuery);
            }
            else if (dataQuery is PricelistAssignmentExportDataQuery pricelistAssignmentExportDataQuery)
            {
                result = new PricelistAssignmentExportPagedDataSource(_pricingService, _pricingSearchService,
                    _catalogService, pricelistAssignmentExportDataQuery);
            }
            else if (dataQuery is PricelistExportDataQuery pricelistExportDataQuery)
            {
                result = new PricelistExportPagedDataSource(_pricingService, _pricingSearchService, pricelistExportDataQuery);
            }

            return result ?? throw new ArgumentException($"Unsupported export query type: {dataQuery.GetType().Name}");
        }
    }
}
