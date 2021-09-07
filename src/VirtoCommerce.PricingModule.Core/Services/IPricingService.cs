using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Core.Services
{
    
    public interface IPricingService
    {
        Task<IEnumerable<Price>> GetPricesByIdAsync(IEnumerable<string> ids);
        Task<IEnumerable<Pricelist>> GetPricelistsByIdAsync(IEnumerable<string> ids);
        Task<IEnumerable<PricelistAssignment>> GetPricelistAssignmentsByIdAsync(IEnumerable<string> ids);

        Task SavePricesAsync(IEnumerable<Price> prices);
        Task SavePricelistsAsync(IEnumerable<Pricelist> priceLists);
        Task SavePricelistAssignmentsAsync(IEnumerable<PricelistAssignment> assignments);

        Task DeletePricelistsAsync(IEnumerable<string> ids);
        Task DeletePricesAsync(IEnumerable<string> ids);
        Task DeletePricelistsAssignmentsAsync(IEnumerable<string> ids);

        /// <summary>
        /// Evaluate pricelists for special context. All resulting pricelists ordered by priority
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        Task<IEnumerable<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext);

        /// <summary>
        /// Evaluation product prices.
        /// Will get either all prices or one price per currency depending on the settings in evalContext.
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        Task<IEnumerable<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext);
    }
}
