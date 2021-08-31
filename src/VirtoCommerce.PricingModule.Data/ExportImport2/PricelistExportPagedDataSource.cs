using System.Linq;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PricelistExportPagedDataSource : ExportPagedDataSource<PricelistExportDataQuery, PricelistSearchCriteria>
    {
        private readonly ISearchService<PricesSearchCriteria, PriceSearchResult, Price> _priceSearchService;
        private readonly ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist> _pricelistSearchService;
        private readonly ICrudService<Pricelist> _pricelistService;
        private readonly PricelistExportDataQuery _dataQuery;

        public PricelistExportPagedDataSource(
            IPriceSearchService priceSearchService,
            IPricelistSearchService pricelistSearchService,
            IPricelistService pricelistService,
            PricelistExportDataQuery dataQuery) : base(dataQuery)
        {
            _priceSearchService = (ISearchService<PricesSearchCriteria, PriceSearchResult, Price>)priceSearchService;
            _pricelistSearchService = (ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist>)pricelistSearchService;
            _pricelistService = (ICrudService<Pricelist>)pricelistService;
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
                result = _pricelistService.GetByIdsAsync(searchCriteria.ObjectIds.ToArray()).GetAwaiter().GetResult().ToArray();
                totalCount = result.Length;
            }
            else
            {
                var pricelistSearchResult = _pricelistSearchService.SearchAsync(searchCriteria).GetAwaiter().GetResult();
                result = pricelistSearchResult.Results.ToArray();
                totalCount = pricelistSearchResult.TotalCount;
            }

            if (!result.IsNullOrEmpty())
            {
                var pricelistIds = result.Select(x => x.Id).ToArray();
                var prices = _priceSearchService.SearchAsync(new PricesSearchCriteria() { PriceListIds = pricelistIds, Take = int.MaxValue }).GetAwaiter().GetResult();
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
