using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule2.Web.Models;

namespace VirtoCommerce.PricingModule2.Web.Entities;

public class Price2Entity : PriceEntity
{
    [Column(TypeName = "Money")]
    public decimal? RecommendedPrice { get; set; }

    public override Price ToModel(Price price)
    {
        if (price is Price2 price2)
        {
            price2.RecommendedPrice = RecommendedPrice;
        }

        base.ToModel(price);

        return price;
    }

    public override PriceEntity FromModel(Price price, PrimaryKeyResolvingMap pkMap)
    {
        if (price is Price2 price2)
        {
            RecommendedPrice = price2.RecommendedPrice;
        }

        base.FromModel(price, pkMap);

        return this;
    }

    public override void Patch(PriceEntity target)
    {
        if (target is Price2Entity target2)
        {
            target2.RecommendedPrice = RecommendedPrice;
        }

        base.Patch(target);
    }
}
