using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Platform.Core.Serialization;
using coreCatalogModel = VirtoCommerce.Domain.Catalog.Model;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using webModel = VirtoCommerce.PricingModule.Web.Model;

namespace VirtoCommerce.PricingModule.Web.Converters
{
    public static class PriceListConverter
    {
        public static webModel.Pricelist ToWebModel(this coreModel.Pricelist priceList, ConditionExpressionTree etalonEpressionTree = null)
        {
            var retVal = new webModel.Pricelist();
            retVal.InjectFrom(priceList);
            retVal.Currency = priceList.Currency;

            if (priceList.Prices != null)
            {
                retVal.ProductPrices = new List<webModel.ProductPrice>();

            }

            if (priceList.Assignments != null)
            {
                retVal.Assignments = priceList.Assignments.Select(x => x.ToWebModel(etalonEpressionTree)).ToList();
            }

            return retVal;
        }

        public static coreModel.Pricelist ToCoreModel(this webModel.Pricelist priceList, IExpressionSerializer expressionSerializer)
        {
            var retVal = new coreModel.Pricelist();
            retVal.InjectFrom(priceList);
            retVal.Currency = priceList.Currency;

            if (priceList.ProductPrices != null)
            {
                retVal.Prices = priceList.ProductPrices.SelectMany(x => x.Prices).Select(x => x.ToCoreModel()).ToList();
            }

            if (priceList.Assignments != null)
            {
                retVal.Assignments = priceList.Assignments.Select(x => x.ToCoreModel(expressionSerializer)).ToList();
            }

            return retVal;
        }
    }
}
