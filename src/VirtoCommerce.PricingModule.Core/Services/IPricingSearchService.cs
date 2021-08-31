using System;
using System.Threading.Tasks;
using VirtoCommerce.PricingModule.Core.Model.Search;

namespace VirtoCommerce.PricingModule.Core.Services
{
    [Obsolete(@"Need to remove after inheriting IPriceSearchService, IPricelistSearchService, and IPricelistAssignmentSearchService from ISearchServiceService.")]
    public interface IPricingSearchService
    {
        Task<PriceSearchResult> SearchPricesAsync(PricesSearchCriteria criteria);
        Task<PricelistSearchResult> SearchPricelistsAsync(PricelistSearchCriteria criteria);
        Task<PricelistAssignmentSearchResult> SearchPricelistAssignmentsAsync(PricelistAssignmentsSearchCriteria criteria);
    }
}
