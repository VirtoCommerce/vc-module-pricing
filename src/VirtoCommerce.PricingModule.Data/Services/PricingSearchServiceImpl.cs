using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingSearchServiceImpl : IPricingSearchService
    {

        private readonly ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment> _pricelistAssignmentSearchService;
        private readonly ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist> _pricelistSearchService;
        private readonly ISearchService<PricesSearchCriteria, PriceSearchResult, Price> _priceSearchService;

        public PricingSearchServiceImpl(ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment> pricelistAssignmentSearchService,
                                        ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist> pricelistSearchService,
                                        ISearchService<PricesSearchCriteria, PriceSearchResult, Price> priceSearchService)
        {
            _pricelistAssignmentSearchService = pricelistAssignmentSearchService;
            _pricelistSearchService = pricelistSearchService;
            _priceSearchService = priceSearchService;
        }

        public Task<PricelistAssignmentSearchResult> SearchPricelistAssignmentsAsync(PricelistAssignmentsSearchCriteria criteria)
        {
            return _pricelistAssignmentSearchService.SearchAsync(criteria);
        }

        public Task<PricelistSearchResult> SearchPricelistsAsync(PricelistSearchCriteria criteria)
        {
            return _pricelistSearchService.SearchAsync(criteria);
        }

        public Task<PriceSearchResult> SearchPricesAsync(PricesSearchCriteria criteria)
        {
            return _priceSearchService.SearchAsync(criteria);
        }
    }
}

