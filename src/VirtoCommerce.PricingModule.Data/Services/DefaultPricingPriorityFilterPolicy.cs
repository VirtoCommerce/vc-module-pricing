using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Services
{
    /// <summary>
    ///  Represents a implementation  for filtering  prices depends on order of the given price lists and other business rules
    /// </summary>
    public class DefaultPricingPriorityFilterPolicy : IPricingPriorityFilterPolicy
    {
        public virtual IEnumerable<Price> FilterPrices(IEnumerable<Price> prices, PriceEvaluationContext evalContext)
        {
            if (prices == null)
            {
                throw new ArgumentNullException(nameof(prices));
            }
            if (evalContext == null)
            {
                throw new ArgumentNullException(nameof(evalContext));
            }

            // If no certain date is set, default to now, because that's probably the intention of the requester
            // and it's backwards compatible.
            var certainDate = evalContext.CertainDate ?? DateTime.UtcNow;
            
            var result = new List<Price>();
            if (evalContext.ReturnAllMatchedPrices)
            {
                // Get all prices, ordered by currency and amount.
                result = prices.OrderBy(x => x.Currency).ThenBy(x => Math.Min(x.Sale ?? x.List, x.List)).ToList();
            }
            else if (!evalContext.Pricelists.IsNullOrEmpty() || !evalContext.PricelistIds.IsNullOrEmpty())
            {
                var priorities = GetPriorities(evalContext);

                foreach (var productPrices in prices.GroupBy(x => x.ProductId))
                {
                    result.AddRange(SelectBestProductPrices(productPrices, priorities, certainDate));
                }
            }
            return result;
        }

        private static Dictionary<string, int> GetPriorities(PriceEvaluationContext evalContext)
        {
            Dictionary<string, int> priorities;

            if (evalContext.Pricelists.IsNullOrEmpty())
            {
                var ids = evalContext.PricelistIds.ToList();

                // As priceListOrdererList is sorted by priority (descending), we save inverted PricelistId's index as Priority
                priorities = ids.ToDictionary(x => x, x => ids.Count - ids.IndexOf(x));
            }
            else
            {
                priorities = evalContext.Pricelists.Distinct().ToDictionary(x => x.Id, x => x.Priority);
            }

            return priorities;
        }

        private static List<Price> SelectBestProductPrices(IGrouping<string, Price> productPrices, Dictionary<string, int> priorities, DateTime certainDate)
        {
            var result = new List<Price>();

            var priceTuples = productPrices
                .Select(x => new
                {
                    Price = x, x.Currency, x.MinQuantity, Priority = priorities.TryGetValue(x.PricelistId, out var priority) ? (int?)priority : null
                })
                .Where(x => x.Priority != null);

            // Group by Currency and by MinQuantity
            foreach (var pricesGroupByCurrency in priceTuples.GroupBy(x => x.Currency))
            {
                var minAcceptablePriority = 0;
                // take prices with lower MinQuantity first
                foreach (var pricesGroupByMinQuantity in pricesGroupByCurrency
                             .GroupBy(x => x.MinQuantity)
                             .OrderBy(x => x.Key))
                {
                    // Take start/end date most close to certainDate, because that price is more specific in time.
                    // Take minimal price from most prioritized Pricelist.
                    var groupAcceptablePrice = pricesGroupByMinQuantity
                        .OrderByDescending(x => x.Priority)
                        .ThenBy(x => certainDate.Subtract(x.Price.StartDate.GetValueOrDefault(DateTime.MinValue)).TotalSeconds)
                        .ThenBy(x => x.Price.EndDate.GetValueOrDefault(DateTime.MaxValue).Subtract(certainDate).TotalSeconds)
                        .ThenBy(x => Math.Min(x.Price.Sale ?? x.Price.List, x.Price.List))
                        .First();

                    if (groupAcceptablePrice.Priority >= minAcceptablePriority)
                    {
                        minAcceptablePriority = groupAcceptablePrice.Priority.Value;
                        result.Add(groupAcceptablePrice.Price);
                    }
                }
            }

            return result;
        }
    }
}
