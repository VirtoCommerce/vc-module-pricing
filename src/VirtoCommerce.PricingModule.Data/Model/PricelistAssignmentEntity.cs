using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Conditions;

namespace VirtoCommerce.PricingModule.Data.Model
{
    public class PricelistAssignmentEntity : AuditableEntity, IHasOuterId, IDataEntity<PricelistAssignmentEntity, PricelistAssignment>
    {
        [StringLength(128)]
        [Required]
        public string Name { get; set; }

        [StringLength(512)]
        public string Description { get; set; }

        public int Priority { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string ConditionExpression { get; set; }

        public string PredicateVisualTreeSerialized { get; set; }

        [StringLength(128)]
        public string CatalogId { get; set; }

        [StringLength(128)]
        public string StoreId { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        #region Navigation Properties

        public string PricelistId { get; set; }
        public virtual PricelistEntity Pricelist { get; set; }

        #endregion

        public virtual PricelistAssignment ToModel(PricelistAssignment assignment)
        {
            ArgumentNullException.ThrowIfNull(assignment);

            assignment.Id = Id;
            assignment.CreatedBy = CreatedBy;
            assignment.CreatedDate = CreatedDate;
            assignment.ModifiedBy = ModifiedBy;
            assignment.ModifiedDate = ModifiedDate;
            assignment.OuterId = OuterId;

            assignment.CatalogId = CatalogId;
            assignment.StoreId = StoreId;
            assignment.Description = Description;
            assignment.Name = Name;
            assignment.PricelistId = PricelistId;
            assignment.Priority = Priority;
            assignment.StartDate = StartDate;
            assignment.EndDate = EndDate;

            if (Pricelist != null)
            {
                //Need to make lightweight pricelist
                assignment.Pricelist = AbstractTypeFactory<Pricelist>.TryCreateInstance();
                assignment.Pricelist.Id = Pricelist.Id;
                assignment.Pricelist.Currency = Pricelist.Currency;
                assignment.Pricelist.Description = Pricelist.Description;
                assignment.Pricelist.Name = Pricelist.Name;

            }
            assignment.DynamicExpression = AbstractTypeFactory<PriceConditionTree>.TryCreateInstance();
            if (PredicateVisualTreeSerialized != null)
            {
                assignment.DynamicExpression = JsonConvert.DeserializeObject<PriceConditionTree>(PredicateVisualTreeSerialized, new ConditionJsonConverter());
            }
            return assignment;
        }

        public virtual PricelistAssignmentEntity FromModel(PricelistAssignment assignment, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(assignment);

            pkMap.AddPair(assignment, this);

            Id = assignment.Id;
            CreatedBy = assignment.CreatedBy;
            CreatedDate = assignment.CreatedDate;
            ModifiedBy = assignment.ModifiedBy;
            ModifiedDate = assignment.ModifiedDate;
            OuterId = assignment.OuterId;

            CatalogId = assignment.CatalogId;
            StoreId = assignment.StoreId;
            Description = assignment.Description;
            Name = assignment.Name;
            PricelistId = assignment.PricelistId;
            Priority = assignment.Priority;
            StartDate = assignment.StartDate;
            EndDate = assignment.EndDate;

            if (assignment.DynamicExpression != null)
            {
                PredicateVisualTreeSerialized = JsonConvert.SerializeObject(assignment.DynamicExpression, new ConditionJsonConverter(doNotSerializeAvailCondition: true));
            }

            return this;
        }

        public virtual void Patch(PricelistAssignmentEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.OuterId = OuterId;
            target.Name = Name;
            target.Description = Description;
            target.CatalogId = CatalogId;
            target.StoreId = StoreId;
            target.PricelistId = PricelistId;
            target.Priority = Priority;
            target.PredicateVisualTreeSerialized = PredicateVisualTreeSerialized;
            target.StartDate = StartDate;
            target.EndDate = EndDate;
        }
    }
}
