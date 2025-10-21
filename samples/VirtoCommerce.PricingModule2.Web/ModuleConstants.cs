using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.PricingModule2.Web;

public class ModuleConstants
{
    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor RecommendedPricePercent { get; } = new()
            {
                Name = "Pricing.RecommendedPricePercent",
                GroupName = "Pricing|General",
                ValueType = SettingValueType.Decimal,
                DefaultValue = 1.2m
            };

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    yield return RecommendedPricePercent;
                }
            }
        }
    }
}
