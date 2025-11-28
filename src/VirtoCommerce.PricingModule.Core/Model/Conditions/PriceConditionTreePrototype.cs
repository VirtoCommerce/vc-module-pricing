using System.Linq;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.CoreModule.Core.Conditions.Browse;
using VirtoCommerce.CoreModule.Core.Conditions.GeoConditions;

namespace VirtoCommerce.PricingModule.Core.Model.Conditions
{
    public class PriceConditionTreePrototype : ConditionTree
    {
        public PriceConditionTreePrototype()
        {
            WithAvailableChildren(new BlockPricingCondition()
                .WithAvailableChildren(
                    new ConditionGeoTimeZone(),
                    new ConditionGeoZipCode(),
                    new ConditionStoreSearchedPhrase(),
                    new ConditionAgeIs(),
                    new ConditionGenderIs(),
                    new ConditionGeoCity(),
                    new ConditionGeoCountry(),
                    new ConditionGeoState(),
                    new ConditionLanguageIs(),
                    new UserGroupsContainsCondition()
                )
            );
            Children = AvailableChildren.ToList();
        }
    }
}
