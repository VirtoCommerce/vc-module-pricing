using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PricelistAssignmentExportPagedDataSource : ExportPagedDataSource<PricelistAssignmentExportDataQuery, PricelistAssignmentsSearchCriteria>
    {
        private readonly IPricelistAssignmentSearchService _pricelistAssignmentSearchService;
        private readonly IPricelistAssignmentService _pricelistAssignmentService;
        private readonly IPricelistService _pricelistService;
        private readonly ICatalogService _catalogService;
        private readonly PricelistAssignmentExportDataQuery _dataQuery;

        public PricelistAssignmentExportPagedDataSource(
            PricelistAssignmentExportDataQuery dataQuery,
            IPricelistAssignmentSearchService pricelistAssignmentSearchService,
            IPricelistAssignmentService pricelistAssignmentService,
            IPricelistService pricelistService,
            ICatalogService catalogService)
            : base(dataQuery)
        {
            _dataQuery = dataQuery;
            _pricelistAssignmentSearchService = pricelistAssignmentSearchService;
            _pricelistAssignmentService = pricelistAssignmentService;
            _pricelistService = pricelistService;
            _catalogService = catalogService;
        }

        protected override PricelistAssignmentsSearchCriteria BuildSearchCriteria(PricelistAssignmentExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.PriceListIds = _dataQuery.PriceListIds;
            result.CatalogIds = _dataQuery.CatalogIds;
            result.Keyword = _dataQuery.Keyword;

            return result;
        }

        protected override ExportableSearchResult FetchData(PricelistAssignmentsSearchCriteria searchCriteria)
        {
            IList<PricelistAssignment> result;
            int totalCount;

            if (searchCriteria.ObjectIds.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                result = _pricelistAssignmentService.GetNoCloneAsync(searchCriteria.ObjectIds).GetAwaiter().GetResult();
                totalCount = result.Count;
            }
            else
            {
                var pricelistAssignmentSearchResult = _pricelistAssignmentSearchService.SearchNoCloneAsync(searchCriteria).GetAwaiter().GetResult();
                result = pricelistAssignmentSearchResult.Results;
                totalCount = pricelistAssignmentSearchResult.TotalCount;
            }

            return new ExportableSearchResult()
            {
                Results = ToExportable(result).ToList(),
                TotalCount = totalCount,
            };
        }

        protected virtual IEnumerable<IExportable> ToExportable(IEnumerable<ICloneable> objects)
        {
            var models = objects.Cast<PricelistAssignment>();
            var viewableMap = models.ToDictionary(x => x, x => AbstractTypeFactory<ExportablePricelistAssignment>.TryCreateInstance().FromModel(x));

            FillAdditionalProperties(viewableMap);

            var modelIds = models.Select(x => x.Id).ToList();
            var result = viewableMap.Values.OrderBy(x => modelIds.IndexOf(x.Id));

            return result;
        }

        protected virtual void FillAdditionalProperties(Dictionary<PricelistAssignment, ExportablePricelistAssignment> viewableMap)
        {
            var models = viewableMap.Keys;
            var catalogIds = models.Select(x => x.CatalogId).Distinct().ToArray();
            var pricelistIds = models.Select(x => x.PricelistId).Distinct().ToArray();
            var catalogs = _catalogService.GetNoCloneAsync(catalogIds, CatalogResponseGroup.Info.ToString()).GetAwaiter().GetResult();
            var pricelists = _pricelistService.GetNoCloneAsync(pricelistIds).GetAwaiter().GetResult();

            foreach (var kvp in viewableMap)
            {
                var model = kvp.Key;
                var viewableEntity = kvp.Value;
                var catalog = catalogs.FirstOrDefault(x => x.Id == model.CatalogId);
                var pricelist = pricelists.FirstOrDefault(x => x.Id == model.PricelistId);

                viewableEntity.CatalogName = catalog?.Name;
                viewableEntity.PricelistName = pricelist?.Name;
            }
        }
    }
}
