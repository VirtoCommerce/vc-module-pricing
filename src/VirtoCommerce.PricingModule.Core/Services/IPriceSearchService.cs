using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;

namespace VirtoCommerce.PricingModule.Core.Services;

public interface IPriceSearchService : ISearchService<PricesSearchCriteria, PriceSearchResult, Price>
{
}
