using FluentValidation;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Validators
{
    public class ProductPriceValidator : AbstractValidator<ProductPrice>
    {
        public ProductPriceValidator(AbstractValidator<Price> priceValidator)
        {
            RuleForEach(x => x.Prices)
                .SetValidator(priceValidator);
        }
    }
}
