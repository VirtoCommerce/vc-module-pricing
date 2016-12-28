using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Serialization;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
namespace VirtoCommerce.PricingModule.Web.Model
{
    public class PricelistAssignment : coreModel.PricelistAssignment
    {	
        /// <summary>
        /// List of conditions and rules to define Prices Assignment is valid
        /// </summary>
        public ConditionExpressionTree DynamicExpression { get; set; }

        public virtual PricelistAssignment FromModel(coreModel.PricelistAssignment assignment, ConditionExpressionTree etalonEpressionTree = null)
        {
            this.Id = assignment.Id;
            this.CatalogId = assignment.CatalogId;
            this.ConditionExpression = assignment.ConditionExpression;
            this.CreatedBy = assignment.CreatedBy;
            this.CreatedDate = assignment.CreatedDate;
            this.Description = assignment.Description;
            this.EndDate = assignment.EndDate;
            this.ModifiedBy = assignment.ModifiedBy;
            this.ModifiedDate = assignment.ModifiedDate;
            this.Name = assignment.Name;
            this.PredicateVisualTreeSerialized = assignment.PredicateVisualTreeSerialized;
            this.PricelistId = assignment.PricelistId;
            this.Priority = assignment.Priority;
            this.StartDate = assignment.StartDate;

            this.Catalog = assignment.Catalog;
            this.Pricelist = assignment.Pricelist;

            this.DynamicExpression = etalonEpressionTree;
            if (!string.IsNullOrEmpty(assignment.PredicateVisualTreeSerialized))
            {
                this.DynamicExpression = JsonConvert.DeserializeObject<ConditionExpressionTree>(assignment.PredicateVisualTreeSerialized);
                if (etalonEpressionTree != null)
                {
                    //Copy available elements from etalon because they not persisted
                    var sourceBlocks = ((DynamicExpression)etalonEpressionTree).Traverse(x => x.Children);
                    var targetBlocks = ((DynamicExpression)this.DynamicExpression).Traverse(x => x.Children).ToList();

                    foreach (var sourceBlock in sourceBlocks)
                    {
                        foreach (var targetBlock in targetBlocks.Where(x => x.Id == sourceBlock.Id))
                        {
                            targetBlock.AvailableChildren = sourceBlock.AvailableChildren;
                        }
                    }

                    //copy available elements from etalon
                    this.DynamicExpression.AvailableChildren = etalonEpressionTree.AvailableChildren;
                }
            }
            return this;
        }

        public virtual coreModel.PricelistAssignment ToModel(coreModel.PricelistAssignment assignment, IExpressionSerializer expressionSerializer)
        {
            assignment.Id = this.Id;
            assignment.CatalogId = this.CatalogId;
            assignment.ConditionExpression = this.ConditionExpression;
            assignment.CreatedBy = this.CreatedBy;
            assignment.CreatedDate = this.CreatedDate;
            assignment.Description = this.Description;
            assignment.EndDate = this.EndDate;
            assignment.ModifiedBy = this.ModifiedBy;
            assignment.ModifiedDate = this.ModifiedDate;
            assignment.Name = this.Name;
            assignment.PredicateVisualTreeSerialized = this.PredicateVisualTreeSerialized;
            assignment.PricelistId = this.PricelistId;
            assignment.Priority = this.Priority;
            assignment.StartDate = this.StartDate;

            if (this.DynamicExpression != null && this.DynamicExpression.Children != null)
            {
                var conditionExpression = this.DynamicExpression.GetConditionExpression();
                assignment.ConditionExpression = expressionSerializer.SerializeExpression(conditionExpression);

                //Clear availableElements in expression (for decrease size)
                this.DynamicExpression.AvailableChildren = null;
                var allBlocks = ((DynamicExpression)this.DynamicExpression).Traverse(x => x.Children);
                foreach (var block in allBlocks)
                {
                    block.AvailableChildren = null;
                }
                assignment.PredicateVisualTreeSerialized = JsonConvert.SerializeObject(this.DynamicExpression);
            }

            return assignment;
        }
    }
}