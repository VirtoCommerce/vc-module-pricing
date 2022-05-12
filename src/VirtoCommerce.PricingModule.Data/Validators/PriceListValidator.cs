using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Validators 
{
    public class PriceListValidator : AbstractValidator<Pricelist>
    {
        private readonly int _descriptionMaxLength = (typeof(PricelistEntity)
                .GetProperty(nameof(PricelistEntity.Description))?
                .GetCustomAttributes(typeof(StringLengthAttribute), false)
                .FirstOrDefault(x => x.GetType() == typeof(StringLengthAttribute)) as StringLengthAttribute)?
            .MaximumLength ?? 0;

        private const string DescriptionMessage = "Description must not be longer than {0} symbols";

        public PriceListValidator()
        {
            RuleFor(x => x.Description)
                .Must(x => x.Length <= _descriptionMaxLength)
                .WithMessage(string.Format(DescriptionMessage, _descriptionMaxLength));
        }
    }
}
