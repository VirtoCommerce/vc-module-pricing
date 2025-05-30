using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Model
{
    public class PriceEntity : AuditableEntity, IHasOuterId, IDataEntity<PriceEntity, Price>
    {
        [Column(TypeName = "Money")]
        public decimal? Sale { get; set; }

        [Required]
        [Column(TypeName = "Money")]
        public decimal List { get; set; }

        [StringLength(128)]
        public string ProductId { get; set; }

        [StringLength(1024)]
        public string ProductName { get; set; }

        public decimal MinQuantity { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        #region Navigation Properties

        [StringLength(128)]
        public string PricelistId { get; set; }

        public virtual PricelistEntity Pricelist { get; set; }

        #endregion

        public virtual Price ToModel(Price price)
        {
            ArgumentNullException.ThrowIfNull(price);

            price.Id = Id;
            price.CreatedBy = CreatedBy;
            price.CreatedDate = CreatedDate;
            price.ModifiedBy = ModifiedBy;
            price.ModifiedDate = ModifiedDate;
            price.OuterId = OuterId;

            price.List = List;
            price.MinQuantity = (int)MinQuantity;
            price.PricelistId = PricelistId;
            price.ProductId = ProductId;
            price.Sale = Sale;
            price.StartDate = StartDate;
            price.EndDate = EndDate;

            if (Pricelist != null)
            {
                price.Currency = Pricelist.Currency;
            }

            return price;
        }

        public virtual PriceEntity FromModel(Price price, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(price);

            pkMap.AddPair(price, this);

            Id = price.Id;
            CreatedBy = price.CreatedBy;
            CreatedDate = price.CreatedDate;
            ModifiedBy = price.ModifiedBy;
            ModifiedDate = price.ModifiedDate;
            OuterId = price.OuterId;

            List = price.List;
            MinQuantity = price.MinQuantity;
            PricelistId = price.PricelistId;
            ProductId = price.ProductId;
            Sale = price.Sale;
            StartDate = price.StartDate;
            EndDate = price.EndDate;

            return this;
        }

        public virtual void Patch(PriceEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.ProductId = ProductId;
            target.List = List;
            target.Sale = Sale;
            target.MinQuantity = MinQuantity;
            target.StartDate = StartDate;
            target.EndDate = EndDate;
            target.OuterId = OuterId;
        }
    }
}
