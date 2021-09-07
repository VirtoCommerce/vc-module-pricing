using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingSearchServiceImpl : IPricingSearchService
    {
        private readonly ISearchService<PricesSearchCriteria, PriceSearchResult, Price> _priceSearchService;
        private readonly ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist> _pricelistSearchService;
        private readonly ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment> _pricelistAssignmentSearchService;

        public PricingSearchServiceImpl(IPriceSearchService priceSearchService
            , IPricelistSearchService pricelistSearchService
            , IPricelistAssignmentSearchService pricelistAssignmentSearchService)
        {
            _priceSearchService = (ISearchService<PricesSearchCriteria, PriceSearchResult, Price>)priceSearchService;
            _pricelistSearchService = (ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist>)pricelistSearchService;
            _pricelistAssignmentSearchService = (ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment>)pricelistAssignmentSearchService;
        }

        public virtual Task<PriceSearchResult> SearchPricesAsync(PricesSearchCriteria criteria)
        {
            return _priceSearchService.SearchAsync(criteria);
        }

        public virtual Task<PricelistSearchResult> SearchPricelistsAsync(PricelistSearchCriteria criteria)
        {
            return _pricelistSearchService.SearchAsync(criteria);
        }

        public virtual Task<PricelistAssignmentSearchResult> SearchPricelistAssignmentsAsync(PricelistAssignmentsSearchCriteria criteria)
        {
            return _pricelistAssignmentSearchService.SearchAsync(criteria);
        }       
    }
}

