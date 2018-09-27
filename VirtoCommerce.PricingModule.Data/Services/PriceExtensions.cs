using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Pricing.Model;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public static class PriceExtensions
    {
        public static IEnumerable<Price> FilterByMostRelevant(this IEnumerable<Price> prices, DateTime certainDate)
        {
            foreach (var pricesForProduct in prices.GroupBy(x => x.ProductId))
            {
                var orderedPrices = pricesForProduct
                    // Startdate closest to certainDate is more important.
                    .OrderBy(x => certainDate.Subtract(x.StartDate.GetValueOrDefault()).TotalSeconds)
                    // Enddate closest to certainDate is more important.
                    .ThenBy(x => x.EndDate.GetValueOrDefault().Subtract(certainDate).TotalSeconds);

                var filteredPrice = orderedPrices.FirstOrDefault();
                if (filteredPrice != null)
                    yield return filteredPrice;
            }
        }
    }
}
