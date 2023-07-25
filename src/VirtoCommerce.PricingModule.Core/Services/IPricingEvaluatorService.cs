using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Core.Services
{
    public interface IPricingEvaluatorService
    {
        /// <summary>
        /// Evaluate pricelists for special context. All resulting pricelists ordered by priority
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        Task<IList<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext);

        /// <summary>
        /// Evaluation product prices.
        /// Will get either all prices or one price per currency depending on the settings in evalContext.
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        Task<IList<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext);
    }
}
