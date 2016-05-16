using System.Linq;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Serialization;
using coreCatalogModel = VirtoCommerce.Domain.Catalog.Model;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using webModel = VirtoCommerce.PricingModule.Web.Model;

namespace VirtoCommerce.PricingModule.Web.Converters
{
    public static class PricelistAssignmentConverter
    {
        public static webModel.PricelistAssignment ToWebModel(this coreModel.PricelistAssignment assignment, coreCatalogModel.Catalog[] catalogs = null, ConditionExpressionTree etalonEpressionTree = null)
        {
            var retVal = new webModel.PricelistAssignment();
            retVal.InjectFrom(assignment);

            if (catalogs != null)
            {
                var catalog = catalogs.FirstOrDefault(x => x.Id == assignment.CatalogId);
                if (catalog != null)
                {
                    retVal.CatalogName = catalog.Name;
                }
            }

            retVal.DynamicExpression = etalonEpressionTree;
            if (!string.IsNullOrEmpty(assignment.PredicateVisualTreeSerialized))
            {
                retVal.DynamicExpression = JsonConvert.DeserializeObject<ConditionExpressionTree>(assignment.PredicateVisualTreeSerialized);
                if (etalonEpressionTree != null)
                {
                    //Copy available elements from etalon because they not persisted
                    var sourceBlocks = ((DynamicExpression)etalonEpressionTree).Traverse(x => x.Children);
                    var targetBlocks = ((DynamicExpression)retVal.DynamicExpression).Traverse(x => x.Children).ToList();

                    foreach (var sourceBlock in sourceBlocks)
                    {
                        foreach (var targetBlock in targetBlocks.Where(x => x.Id == sourceBlock.Id))
                        {
                            targetBlock.AvailableChildren = sourceBlock.AvailableChildren;
                        }
                    }

                    //copy available elements from etalon
                    retVal.DynamicExpression.AvailableChildren = etalonEpressionTree.AvailableChildren;
                }
            }

            return retVal;
        }

        public static coreModel.PricelistAssignment ToCoreModel(this webModel.PricelistAssignment assignment, IExpressionSerializer expressionSerializer)
        {
            var retVal = new coreModel.PricelistAssignment();
            retVal.InjectFrom(assignment);

            if (assignment.DynamicExpression != null && assignment.DynamicExpression.Children != null)
            {
                var conditionExpression = assignment.DynamicExpression.GetConditionExpression();
                retVal.ConditionExpression = expressionSerializer.SerializeExpression(conditionExpression);

                //Clear availableElements in expression (for decrease size)
                assignment.DynamicExpression.AvailableChildren = null;
                var allBlocks = ((DynamicExpression)assignment.DynamicExpression).Traverse(x => x.Children);
                foreach (var block in allBlocks)
                {
                    block.AvailableChildren = null;
                }
                retVal.PredicateVisualTreeSerialized = JsonConvert.SerializeObject(assignment.DynamicExpression);
            }

            return retVal;
        }
    }
}
