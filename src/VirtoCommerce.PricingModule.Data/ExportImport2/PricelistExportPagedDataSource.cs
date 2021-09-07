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
        private readonly PricelistExportDataQuery _dataQuery;

        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;

        public PricelistExportPagedDataSource(IPricingService pricingService
            , IPricingSearchService pricingSearchService
            , PricelistExportDataQuery dataQuery) : base(dataQuery)
        {
            _pricingSearchService = pricingSearchService;
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
                result = _pricingService.GetPricelistsByIdAsync(searchCriteria.ObjectIds).GetAwaiter().GetResult().ToArray();
                totalCount = result.Length;
            }
            else
            {
                var pricelistSearchResult = _pricingSearchService.SearchPricelistsAsync(searchCriteria).GetAwaiter().GetResult();
                result = pricelistSearchResult.Results.ToArray();
                totalCount = pricelistSearchResult.TotalCount;
            }

            if (!result.IsNullOrEmpty())
            {
                var pricelistIds = result.Select(x => x.Id).ToArray();
                var prices = _pricingSearchService.SearchPricesAsync(new PricesSearchCriteria() { PriceListIds = pricelistIds, Take = int.MaxValue }).GetAwaiter().GetResult();
                foreach (var pricelist in result)
                {
                    pricelist.Prices = prices.Results.Where(x => x.PricelistId == pricelist.Id).ToArray();
                }
            }

            return new ExportableSearchResult()
            {
                Results = result.Select(x => (IExportable)AbstractTypeFactory<ExportablePricelist>.TryCreateInstance().FromModel(x)).ToList(),
                TotalCount = totalCount,
            };
        }
    }
}
