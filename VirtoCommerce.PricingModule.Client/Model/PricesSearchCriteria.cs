using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VirtoCommerce.PricingModule.Client.Model
{
    /// <summary>
    /// PricesSearchCriteria
    /// </summary>
    [DataContract]
    public partial class PricesSearchCriteria :  IEquatable<PricesSearchCriteria>
    {
        /// <summary>
        /// Gets or Sets GroupByProducts
        /// </summary>
        [DataMember(Name="groupByProducts", EmitDefaultValue=false)]
        public bool? GroupByProducts { get; set; }

        /// <summary>
        /// Gets or Sets PriceListId
        /// </summary>
        [DataMember(Name="priceListId", EmitDefaultValue=false)]
        public string PriceListId { get; set; }

        /// <summary>
        /// Gets or Sets PriceListIds
        /// </summary>
        [DataMember(Name="priceListIds", EmitDefaultValue=false)]
        public List<string> PriceListIds { get; set; }

        /// <summary>
        /// Gets or Sets ProductId
        /// </summary>
        [DataMember(Name="productId", EmitDefaultValue=false)]
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or Sets ProductIds
        /// </summary>
        [DataMember(Name="productIds", EmitDefaultValue=false)]
        public List<string> ProductIds { get; set; }

        /// <summary>
        /// Gets or Sets Keyword
        /// </summary>
        [DataMember(Name="keyword", EmitDefaultValue=false)]
        public string Keyword { get; set; }

        /// <summary>
        /// Gets or Sets Sort
        /// </summary>
        [DataMember(Name="sort", EmitDefaultValue=false)]
        public string Sort { get; set; }

        /// <summary>
        /// Gets or Sets SortInfos
        /// </summary>
        [DataMember(Name="sortInfos", EmitDefaultValue=false)]
        public List<SortInfo> SortInfos { get; set; }

        /// <summary>
        /// Gets or Sets Skip
        /// </summary>
        [DataMember(Name="skip", EmitDefaultValue=false)]
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or Sets Take
        /// </summary>
        [DataMember(Name="take", EmitDefaultValue=false)]
        public int? Take { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PricesSearchCriteria {\n");
            sb.Append("  GroupByProducts: ").Append(GroupByProducts).Append("\n");
            sb.Append("  PriceListId: ").Append(PriceListId).Append("\n");
            sb.Append("  PriceListIds: ").Append(PriceListIds).Append("\n");
            sb.Append("  ProductId: ").Append(ProductId).Append("\n");
            sb.Append("  ProductIds: ").Append(ProductIds).Append("\n");
            sb.Append("  Keyword: ").Append(Keyword).Append("\n");
            sb.Append("  Sort: ").Append(Sort).Append("\n");
            sb.Append("  SortInfos: ").Append(SortInfos).Append("\n");
            sb.Append("  Skip: ").Append(Skip).Append("\n");
            sb.Append("  Take: ").Append(Take).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            return this.Equals(obj as PricesSearchCriteria);
        }

        /// <summary>
        /// Returns true if PricesSearchCriteria instances are equal
        /// </summary>
        /// <param name="other">Instance of PricesSearchCriteria to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PricesSearchCriteria other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    this.GroupByProducts == other.GroupByProducts ||
                    this.GroupByProducts != null &&
                    this.GroupByProducts.Equals(other.GroupByProducts)
                ) && 
                (
                    this.PriceListId == other.PriceListId ||
                    this.PriceListId != null &&
                    this.PriceListId.Equals(other.PriceListId)
                ) && 
                (
                    this.PriceListIds == other.PriceListIds ||
                    this.PriceListIds != null &&
                    this.PriceListIds.SequenceEqual(other.PriceListIds)
                ) && 
                (
                    this.ProductId == other.ProductId ||
                    this.ProductId != null &&
                    this.ProductId.Equals(other.ProductId)
                ) && 
                (
                    this.ProductIds == other.ProductIds ||
                    this.ProductIds != null &&
                    this.ProductIds.SequenceEqual(other.ProductIds)
                ) && 
                (
                    this.Keyword == other.Keyword ||
                    this.Keyword != null &&
                    this.Keyword.Equals(other.Keyword)
                ) && 
                (
                    this.Sort == other.Sort ||
                    this.Sort != null &&
                    this.Sort.Equals(other.Sort)
                ) && 
                (
                    this.SortInfos == other.SortInfos ||
                    this.SortInfos != null &&
                    this.SortInfos.SequenceEqual(other.SortInfos)
                ) && 
                (
                    this.Skip == other.Skip ||
                    this.Skip != null &&
                    this.Skip.Equals(other.Skip)
                ) && 
                (
                    this.Take == other.Take ||
                    this.Take != null &&
                    this.Take.Equals(other.Take)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                int hash = 41;
                // Suitable nullity checks etc, of course :)

                if (this.GroupByProducts != null)
                    hash = hash * 59 + this.GroupByProducts.GetHashCode();

                if (this.PriceListId != null)
                    hash = hash * 59 + this.PriceListId.GetHashCode();

                if (this.PriceListIds != null)
                    hash = hash * 59 + this.PriceListIds.GetHashCode();

                if (this.ProductId != null)
                    hash = hash * 59 + this.ProductId.GetHashCode();

                if (this.ProductIds != null)
                    hash = hash * 59 + this.ProductIds.GetHashCode();

                if (this.Keyword != null)
                    hash = hash * 59 + this.Keyword.GetHashCode();

                if (this.Sort != null)
                    hash = hash * 59 + this.Sort.GetHashCode();

                if (this.SortInfos != null)
                    hash = hash * 59 + this.SortInfos.GetHashCode();

                if (this.Skip != null)
                    hash = hash * 59 + this.Skip.GetHashCode();

                if (this.Take != null)
                    hash = hash * 59 + this.Take.GetHashCode();

                return hash;
            }
        }
    }
}
