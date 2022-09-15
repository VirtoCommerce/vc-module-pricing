using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PricingModule.Core.Model
{
    public class MergedPrice : Entity
    {
        public string Currency { get; set; }

        public string PricelistId { get; set; }

        public string ProductId { get; set; }

        public decimal? Sale { get; set; }

        public decimal List { get; set; }

        public int MinQuantity { get; set; }

        public MergedPriceState State { get; set; }
    }
}
