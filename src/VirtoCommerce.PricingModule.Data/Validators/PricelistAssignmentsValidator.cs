using System.Collections.Generic;
using FluentValidation;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Validators
{
    public class PricelistAssignmentsValidator : AbstractValidator<IEnumerable<PricelistAssignment>>
    {
        public PricelistAssignmentsValidator()
        {
            RuleForEach(a => a)
                .Must(a =>
                {
                    return (string.IsNullOrEmpty(a.CatalogId) || string.IsNullOrEmpty(a.StoreId)) &&
                        (!string.IsNullOrEmpty(a.CatalogId) || !string.IsNullOrEmpty(a.StoreId));
                })
                .WithMessage("Only CatalogId or StoreId should be filled. Both can't be null.");
        }
    }
}
