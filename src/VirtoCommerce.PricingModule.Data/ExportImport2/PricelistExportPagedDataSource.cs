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
        private readonly IPricingSearchService _searchService;
        private readonly IPricingService _pricingService;
        private readonly PricelistExportDataQuery _dataQuery;

        public PricelistExportPagedDataSource(
            IPricingSearchService searchService,
            IPricingService pricingService,
            PricelistExportDataQuery dataQuery) : base(dataQuery)
        {
            _searchService = searchService;
            _pricingService = pricingService;
            _dataQuery = dataQuery;
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
            Pricelist[] result;
            int totalCount;

            if (searchCriteria.ObjectIds.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                result = _pricingService.GetPricelistsByIdAsync(searchCriteria.ObjectIds.ToArray()).GetAwaiter().GetResult();
                totalCount = result.Length;
            }
            else
            {
                var pricelistSearchResult = _searchService.SearchPricelistsAsync(searchCriteria).GetAwaiter().GetResult();
                result = pricelistSearchResult.Results.ToArray();
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
