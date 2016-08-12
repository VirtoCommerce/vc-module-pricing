using Omu.ValueInjecter;
using VirtoCommerce.Platform.Core.Assets;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using webModel = VirtoCommerce.PricingModule.Web.Model;

namespace VirtoCommerce.PricingModule.Web.Converters
{
	public static class PriceConverter
	{
		public static webModel.Price ToWebModel(this coreModel.Price price)
		{
			var retVal = new webModel.Price();
			retVal.InjectFrom(price);
			retVal.Currency = price.Currency;
            if (price.Pricelist != null)
            {
                retVal.PriceListName = price.Pricelist.Name;
            }       
			return retVal;
		}

		public static coreModel.Price ToCoreModel(this webModel.Price price)
		{
			var retVal = new coreModel.Price();
			retVal.InjectFrom(price);
			retVal.Currency = price.Currency;
			return retVal;
		}


	}
}
