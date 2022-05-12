using FluentValidation;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Validators 
{
    public class PriceListValidator : AbstractValidator<Pricelist>
    {
        private const string DescriptionMessage = "Description must not be longer than 512 symbols";

        public PriceListValidator()
        {
            RuleFor(x => x.Description)
                .Must(x => x.Length < 512)
                .WithMessage(DescriptionMessage);
        }
    }
}
