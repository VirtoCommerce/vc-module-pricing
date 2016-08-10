using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using System.Collections.ObjectModel;
using dataModel = VirtoCommerce.PricingModule.Data.Model;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PricingModule.Data.Converters
{
	public static class PriceListAssignmentConverter
	{
		/// <summary>
		/// Converting to model type
		/// </summary>
		/// <param name="catalogBase"></param>
		/// <returns></returns>
		public static coreModel.PricelistAssignment ToCoreModel(this dataModel.PricelistAssignment dbEntity)
		{
			if (dbEntity == null)
				throw new ArgumentNullException("dbEntity");

			var retVal = new coreModel.PricelistAssignment();
			retVal.InjectFrom(dbEntity);
            if(dbEntity.Pricelist != null)
            {
                retVal.Pricelist = dbEntity.Pricelist.ToCoreModel();
            }
            if (!string.IsNullOrEmpty(retVal.ConditionExpression))
            {
                //Temporary back data compatibility fix for serialized expressions
                retVal.ConditionExpression = retVal.ConditionExpression.Replace("VirtoCommerce.DynamicExpressionModule.", "VirtoCommerce.DynamicExpressionsModule.");
            }
            if (!string.IsNullOrEmpty(retVal.PredicateVisualTreeSerialized))
            {
                //Temporary back data compatibility fix for serialized expressions
                retVal.PredicateVisualTreeSerialized = retVal.PredicateVisualTreeSerialized.Replace("VirtoCommerce.DynamicExpressionModule.", "VirtoCommerce.DynamicExpressionsModule.");
            }
            return retVal;

		}


		public static dataModel.PricelistAssignment ToDataModel(this coreModel.PricelistAssignment assignment, PrimaryKeyResolvingMap pkMap)
		{
			if (assignment == null)
				throw new ArgumentNullException("assignment");

			var retVal = new dataModel.PricelistAssignment();
            pkMap.AddPair(assignment, retVal);

            retVal.InjectFrom(assignment);
            if (string.IsNullOrEmpty(retVal.Id))
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
		public static void Patch(this dataModel.PricelistAssignment source, dataModel.PricelistAssignment target)
		{
			if (target == null)
				throw new ArgumentNullException("target");
			var patchInjection = new PatchInjection<dataModel.PricelistAssignment>(x => x.Name, x => x.Description,
																						 x => x.StartDate, x => x.EndDate, x => x.CatalogId,
																						 x => x.PricelistId, x => x.Priority, x => x.ConditionExpression, x=> x.PredicateVisualTreeSerialized);
			target.InjectFrom(patchInjection, source);
		}


	}
}
