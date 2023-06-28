using System;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PricingExportPagedDataSourceFactory : IPricingExportPagedDataSourceFactory
    {
        private readonly IPriceSearchService _priceSearchService;
        private readonly IPriceService _priceService;
        private readonly IPricelistSearchService _pricelistSearchService;
        private readonly IPricelistService _pricelistService;
        private readonly IPricelistAssignmentSearchService _pricelistAssignmentSearchService;
        private readonly IPricelistAssignmentService _pricelistAssignmentService;
        private readonly IItemService _itemService;
        private readonly ICatalogService _catalogService;

        public PricingExportPagedDataSourceFactory(
            IPriceSearchService priceSearchService,
            IPriceService priceService,
            IPricelistSearchService pricelistSearchService,
            IPricelistService pricelistService,
            IPricelistAssignmentSearchService pricelistAssignmentSearchService,
            IPricelistAssignmentService pricelistAssignmentService,
            IItemService itemService,
            ICatalogService catalogService)
        {
            _priceSearchService = priceSearchService;
            _priceService = priceService;
            _pricelistSearchService = pricelistSearchService;
            _pricelistService = pricelistService;
            _pricelistAssignmentSearchService = pricelistAssignmentSearchService;
            _pricelistAssignmentService = pricelistAssignmentService;
            _itemService = itemService;
            _catalogService = catalogService;
        }

        public virtual IPagedDataSource Create(ExportDataQuery dataQuery)
        {
            IPagedDataSource result = null;

            if (dataQuery is PriceExportDataQuery priceExportDataQuery)
            {
                result = new PriceExportPagedDataSource(priceExportDataQuery, _priceSearchService, _priceService, _pricelistService, _itemService);
            }
            else if (dataQuery is PricelistExportDataQuery pricelistExportDataQuery)
            {
                result = new PricelistExportPagedDataSource(pricelistExportDataQuery, _pricelistSearchService, _pricelistService);
            }
            else if (dataQuery is PricelistAssignmentExportDataQuery pricelistAssignmentExportDataQuery)
            {
                result = new PricelistAssignmentExportPagedDataSource(pricelistAssignmentExportDataQuery, _pricelistAssignmentSearchService, _pricelistAssignmentService, _pricelistService, _catalogService);
            }

            return result ?? throw new ArgumentException($"Unsupported export query type: {dataQuery.GetType().Name}");
        }
    }
}
