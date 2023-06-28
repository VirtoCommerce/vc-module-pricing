using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PricelistExportPagedDataSource : ExportPagedDataSource<PricelistExportDataQuery, PricelistSearchCriteria>
    {
        private readonly IPricelistSearchService _pricelistSearchService;
        private readonly IPricelistService _pricelistService;
        private readonly PricelistExportDataQuery _dataQuery;

        public PricelistExportPagedDataSource(
            PricelistExportDataQuery dataQuery,
            IPricelistSearchService pricelistSearchService,
            IPricelistService pricelistService)
            : base(dataQuery)
        {
            _dataQuery = dataQuery;
            _pricelistSearchService = pricelistSearchService;
            _pricelistService = pricelistService;
        }

        protected override PricelistSearchCriteria BuildSearchCriteria(PricelistExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.Currencies = _dataQuery.Currencies;
            result.Keyword = _dataQuery.Keyword;

            return result;
        }

        protected override ExportableSearchResult FetchData(PricelistSearchCriteria searchCriteria)
        {
            IList<Pricelist> result;
            int totalCount;

            if (searchCriteria.ObjectIds.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                result = _pricelistService.GetNoCloneAsync(searchCriteria.ObjectIds).GetAwaiter().GetResult();
                totalCount = result.Count;
            }
            else
            {
                var pricelistSearchResult = _pricelistSearchService.SearchNoCloneAsync(searchCriteria).GetAwaiter().GetResult();
                result = pricelistSearchResult.Results;
                totalCount = pricelistSearchResult.TotalCount;
            }

            return new ExportableSearchResult()
            {
                Results = result.Select(x => (IExportable)AbstractTypeFactory<ExportablePricelist>.TryCreateInstance().FromModel(x)).ToList(),
                TotalCount = totalCount,
            };
        }
    }
}
