using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Model
{
    public class MergedPriceGroupEntity
    {
        public string ProductId { get; set; }

        public int GroupPricesCount { get; set; }

        public int GroupState { get; set; }

        public decimal? MinSalePrice { get; set; }
        public decimal? MaxSalePrice { get; set; }

        public decimal MinListPrice { get; set; }
        public decimal MaxListPrice { get; set; }

        public MergedPriceGroup ToModel(MergedPriceGroup model)
        {
            model.ProductId = ProductId;
            model.GroupPricesCount = GroupPricesCount;
            model.GroupState = (MergedPriceGroupState)GroupState;
            model.MinSalePrice = MinSalePrice;
            model.MaxSalePrice = MaxSalePrice;
            model.MinListPrice = MinListPrice;
            model.MaxListPrice = MaxListPrice;

            return model;
        }
    }
}
