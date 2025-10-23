using System.Collections.Generic;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule2.Web.Models;

namespace VirtoCommerce.PricingModule2.Web.Extensions;

public static class PriceExtensions
{
    private static void FillRecommendedPrice(this Price2 price2, decimal recommendedPricePercent)
    {
        if (price2.Sale != null)
        {
            price2.RecommendedPrice = price2.Sale.Value * recommendedPricePercent;
        }
        else
        {
            price2.RecommendedPrice = price2.List * recommendedPricePercent;
        }
    }

    public static IList<Price> FillRecommendedPrice(this IList<Price> prices, decimal recommendedPricePercent)
    {
        foreach (var price in prices)
        {
            if (price is Price2 { RecommendedPrice: null } price2)
            {
                price2.FillRecommendedPrice(recommendedPricePercent);
            }
        }

        return prices;
    }
}
