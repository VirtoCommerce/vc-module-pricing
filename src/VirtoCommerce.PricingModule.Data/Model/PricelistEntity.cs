using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Model
{
    public class PricelistEntity : AuditableEntity, IHasOuterId, IDataEntity<PricelistEntity, Pricelist>
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(512)]
        public string Description { get; set; }

        [Required]
        [StringLength(64)]
        public string Currency { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        #region Navigation Properties

        public virtual ObservableCollection<PriceEntity> Prices { get; set; }
            = new NullCollection<PriceEntity>();

        public virtual ObservableCollection<PricelistAssignmentEntity> Assignments { get; set; }
            = new NullCollection<PricelistAssignmentEntity>();

        #endregion

        public virtual Pricelist ToModel(Pricelist pricelist)
        {
            ArgumentNullException.ThrowIfNull(pricelist);

            pricelist.Id = Id;
            pricelist.CreatedBy = CreatedBy;
            pricelist.CreatedDate = CreatedDate;
            pricelist.ModifiedBy = ModifiedBy;
            pricelist.ModifiedDate = ModifiedDate;
            pricelist.OuterId = OuterId;

            pricelist.Currency = Currency;
            pricelist.Description = Description;
            pricelist.Name = Name;

            pricelist.Assignments = new List<PricelistAssignment>();
            //Create lightweight assignment for represent assignment info in pricelist
            foreach (var assignmentEntity in Assignments)
            {
                var assignment = AbstractTypeFactory<PricelistAssignment>.TryCreateInstance();
                assignment.Id = assignmentEntity.Id;
                assignment.CatalogId = assignmentEntity.CatalogId;
                assignment.StoreId = assignmentEntity.StoreId;
                assignment.Description = assignmentEntity.Description;
                assignment.Name = assignmentEntity.Name;
                assignment.Priority = assignmentEntity.Priority;
                assignment.StartDate = assignmentEntity.StartDate;
                assignment.EndDate = assignmentEntity.EndDate;

                pricelist.Assignments.Add(assignment);
            }
            return pricelist;
        }

        public virtual PricelistEntity FromModel(Pricelist pricelist, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(pricelist);

            pkMap.AddPair(pricelist, this);

            Id = pricelist.Id;
            CreatedBy = pricelist.CreatedBy;
            CreatedDate = pricelist.CreatedDate;
            ModifiedBy = pricelist.ModifiedBy;
            ModifiedDate = pricelist.ModifiedDate;
            OuterId = pricelist.OuterId;

            Currency = pricelist.Currency;
            Description = pricelist.Description;
            Name = pricelist.Name;

            return this;
        }

        public virtual void Patch(PricelistEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.OuterId = OuterId;
            target.Name = Name;
            target.Currency = Currency;
            target.Description = Description;
        }
    }
}
