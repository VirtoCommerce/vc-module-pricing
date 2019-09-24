using System.Linq;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;

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
                result = _pricingService.GetPricelistsById(searchCriteria.ObjectIds.ToArray());
                totalCount = result.Length;
            }
            else
            {
                var pricelistSearchResult = _searchService.SearchPricelists(searchCriteria);
                result = pricelistSearchResult.Results.ToArray();
                totalCount = pricelistSearchResult.TotalCount;
            }

            if (!result.IsNullOrEmpty())
            {
                var pricelistIds = result.Select(x => x.Id).ToArray();
                var prices = _searchService.SearchPrices(new PricesSearchCriteria() { PriceListIds = pricelistIds, Take = int.MaxValue });
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
