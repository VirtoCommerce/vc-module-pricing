using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.PricingModule.Data.Search
{
    /// <summary>
    /// Represents abstraction used for getting the pricing changes that are used in indexation process
    /// </summary>
    public interface IPricingDocumentChangesProvider : IIndexDocumentChangesProvider
    {
        /// <summary>
        /// The method is used to get prices which have a defined from/till dates and are included in given interval.
        /// </summary>
        Task<GenericSearchResult<IndexDocumentChange>> GetCalendarChangesAsync(DateTime startDate, DateTime endDate, int skip, int take);
    }
}
