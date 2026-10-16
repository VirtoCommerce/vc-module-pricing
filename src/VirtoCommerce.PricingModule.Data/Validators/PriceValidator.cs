using FluentValidation;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Validators
{
    public class PriceValidator : AbstractValidator<Price>
    {
        public PriceValidator()
        {
            RuleFor(x => x.List)
                .GreaterThan(0)
                .WithMessage("List price must be greater than 0");

            RuleFor(x => x.Sale)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Sale price must not be negative")
                .LessThanOrEqualTo(x => x.List)
                .WithMessage("Sale price should not exceed List price")
                .When(x => x.Sale.HasValue);

            RuleFor(x => x.MinQuantity)
                .GreaterThan(0)
                .WithMessage("Min quantity must be greater than 0");
        }
    }
}
