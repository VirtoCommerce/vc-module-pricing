using System;
using System.Threading.Tasks;
using VirtoCommerce.PricingModule.Core.Model.Search;

namespace VirtoCommerce.PricingModule.Core.Services
{
    [Obsolete("Use separate interfaces: ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment>, " +
    "ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist>, " +
    "ISearchService<PricesSearchCriteria, PriceSearchResult, Price>")]
    public interface IPricingSearchService
    {
        Task<PriceSearchResult> SearchPricesAsync(PricesSearchCriteria criteria);
        Task<PricelistSearchResult> SearchPricelistsAsync(PricelistSearchCriteria criteria);
        Task<PricelistAssignmentSearchResult> SearchPricelistAssignmentsAsync(PricelistAssignmentsSearchCriteria criteria);
    }
}
