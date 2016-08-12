using System;
using System.Linq;
using System.Collections.Generic;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using dataModel = VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Converters
{
    public static class PriceConverter
    {
        /// <summary>
        /// Converting to model type
        /// </summary>
        /// <param name="dbEntity"></param>
        /// <returns></returns>
        public static coreModel.Price ToCoreModel(this dataModel.Price dbEntity, IEnumerable<CatalogProduct> products = null)
        {
            if (dbEntity == null)
                throw new ArgumentNullException("dbEntity");

            var retVal = new coreModel.Price();

            retVal.InjectFrom(dbEntity);

            retVal.ProductId = dbEntity.ProductId;
            retVal.MinQuantity = (int)dbEntity.MinQuantity;

            if (dbEntity.Pricelist != null)
            {
                retVal.Currency = dbEntity.Pricelist.Currency;
                retVal.Pricelist = dbEntity.Pricelist.ToCoreModel();
            }

            if(!products.IsNullOrEmpty())
            {
                retVal.Product = products.FirstOrDefault(x => x.Id == retVal.ProductId);
            }

            return retVal;

        }


        public static dataModel.Price ToDataModel(this coreModel.Price price, PrimaryKeyResolvingMap pkMap)
        {
            if (price == null)
                throw new ArgumentNullException("price");

            var retVal = new dataModel.Price();
            pkMap.AddPair(price, retVal);
            retVal.InjectFrom(price);
            retVal.ProductId = price.ProductId;
            retVal.MinQuantity = price.MinQuantity;
            if(string.IsNullOrEmpty(retVal.Id))
            {
                retVal.Id = null;
            }
            return retVal;
        }

        /// <summary>
        /// Patch changes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void Patch(this dataModel.Price source, dataModel.Price target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            var patchInjection = new PatchInjection<dataModel.Price>(true, x => x.ProductId, x => x.List,
                                                                           x => x.MinQuantity, x => x.Sale);
            target.InjectFrom(patchInjection, source);
        }


    }
}
