
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Core.Services
{
    public interface IPricelistAssignmentService
    {
        /// <summary>
        /// Evaluate pricelists for special context. All resulting pricelists ordered by priority
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        Task<IEnumerable<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext);
        Task<PricelistAssignment[]> GetAllPricelistAssignments();
    }
}
