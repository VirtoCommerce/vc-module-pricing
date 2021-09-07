using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Core.Services
{
    public interface IPriceService
    {
        /// <summary>
        /// Evaluation product prices.
        /// Will get either all prices or one price per currency depending on the settings in evalContext.
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        Task<IEnumerable<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext);
    }
}
