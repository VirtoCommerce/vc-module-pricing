using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Model
{
    public class MergedPriceEntity : Entity
    {
        public decimal? Sale { get; set; }

        public decimal List { get; set; }

        public string ProductId { get; set; }

        public decimal MinQuantity { get; set; }

        public string PricelistId { get; set; }

        public int State { get; set; }

        public MergedPrice ToModel(MergedPrice model)
        {
            model.Id = Id;

            model.List = List;
            model.MinQuantity = (int)MinQuantity;
            model.PricelistId = PricelistId;
            model.ProductId = ProductId;
            model.Sale = Sale;
            model.State = (MergedPriceGroupState)State;

            return model;
        }
    }
}
