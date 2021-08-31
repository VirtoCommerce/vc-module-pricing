using System;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PricingExportPagedDataSourceFactory : IPricingExportPagedDataSourceFactory
    {
        private readonly IPriceService _priceService;
        private readonly IPricelistService _pricelistService;
        private readonly IPricelistAssignmentService _pricelistAssignmentService;
        private readonly IPriceSearchService _priceSearchService;
        private readonly IPricelistSearchService _pricelistSearchService;
        private readonly IPricelistAssignmentSearchService _pricelistAssignmentSearchService;

        private readonly IItemService _itemService;
        private readonly ICatalogService _catalogService;

        public PricingExportPagedDataSourceFactory(IPriceService priceService
            , IPriceSearchService priceSearchService
            , IPricelistService pricelistService
            , IPricelistSearchService pricelistSearchService
            , IPricelistAssignmentService pricelistAssignmentService
            , IPricelistAssignmentSearchService pricelistAssignmentSearchService
            , IItemService itemService, ICatalogService catalogService)
        {
            _priceSearchService = priceSearchService;
            _pricelistSearchService = pricelistSearchService;
            _pricelistAssignmentSearchService = pricelistAssignmentSearchService;
            _priceService = priceService;
            _pricelistService = pricelistService;
            _pricelistAssignmentService = pricelistAssignmentService;
            _itemService = itemService;
            _catalogService = catalogService;
        }

        public virtual IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            IPagedDataSource result = null;

            if (dataQuery is PriceExportDataQuery priceExportDataQuery)
            {
                result = new PriceExportPagedDataSource(_priceSearchService, _priceService, _pricelistService, _itemService, priceExportDataQuery);
            }
            else if (dataQuery is PricelistAssignmentExportDataQuery pricelistAssignmentExportDataQuery)
            {
                result = new PricelistAssignmentExportPagedDataSource(_pricelistAssignmentService, _pricelistAssignmentSearchService, _pricelistService,
                    _catalogService, pricelistAssignmentExportDataQuery);
            }
            else if (dataQuery is PricelistExportDataQuery pricelistExportDataQuery)
            {
                result = new PricelistExportPagedDataSource(_priceSearchService, _pricelistSearchService, _pricelistService, pricelistExportDataQuery);
            }

            return result ?? throw new ArgumentException($"Unsupported export query type: {dataQuery.GetType().Name}");
        }
    }
}
