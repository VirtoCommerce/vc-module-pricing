using System.Threading.Tasks;
using VirtoCommerce.PricingModule.Core.Model.Search;

namespace VirtoCommerce.PricingModule.Core.Services
{
    public interface IMergedPriceSearchService
    {
        Task<MergedPriceSearchResult> SearchGroupPricesAsync(MergedPriceSearchCriteria criteria);

        Task<MergedPriceGroupSearchResult> SearchGroupsAsync(MergedPriceSearchCriteria criteria);
    }
}
