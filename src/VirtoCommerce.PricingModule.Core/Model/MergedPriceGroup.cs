namespace VirtoCommerce.PricingModule.Core.Model
{
    public class MergedPriceGroup
    {
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        public string ProductImgSrc { get; set; }

        public int GroupPricesCount { get; set; }

        public MergedPriceState GroupState { get; set; }

        public decimal? MinSalePrice { get; set; }
        public decimal? MaxSalePrice { get; set; }

        public decimal MinListPrice { get; set; }
        public decimal MaxListPrice { get; set; }
    }
}
