using VirtoCommerce.PricingModule2.Web.Models;

namespace VirtoCommerce.PricingModule2.Web.Extensions;

public static class PriceExtensions
{
    public static void FillRecommendedPrice(this Price2 price2, decimal recommendedPricePercent)
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
}
