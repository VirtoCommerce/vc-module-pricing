using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.PricingModule.Core
{
    [ExcludeFromCodeCoverage]
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Read = "pricing:read";
                public const string Create = "pricing:create";
                public const string Access = "pricing:access";
                public const string Update = "pricing:update";
                public const string Delete = "pricing:delete";
                public const string Export = "pricing:export";

                public static string[] AllPermissions { get; } = { Read, Create, Access, Update, Delete, Export };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public const string PriceIndexingValueMax = "Max";
                public const string PriceIndexingValueMin = "Min";

                public static SettingDescriptor ExportImportPageSize { get; } = new SettingDescriptor
                {
                    Name = "Pricing.ExportImport.PageSize",
                    GroupName = "Pricing|General",
                    ValueType = SettingValueType.PositiveInteger,
                    DefaultValue = 50
                };

                public static SettingDescriptor IndexationDatePricingCalendar { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Search.IndexingJobs.IndexationDate.Pricing.Calendar",
                    GroupName = "Pricing|Search",
                    ValueType = SettingValueType.DateTime,
                    DefaultValue = default(DateTime),
                    IsHidden = true
                };

                public static SettingDescriptor PricingIndexing { get; } = new SettingDescriptor
                {
                    Name = "Pricing.Indexing.Enable",
                    GroupName = "Pricing|Search",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true
                };

                public static SettingDescriptor EventBasedIndexation { get; } = new SettingDescriptor
                {
                    Name = "Pricing.Search.EventBasedIndexation.Enable",
                    GroupName = "Pricing|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true
                };


                public static SettingDescriptor PriceEvaluationCacheEnabled { get; } = new SettingDescriptor
                {
                    Name = "Pricing.Evaluation.Cache.Enable",
                    GroupName = "Pricing|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true,
                };

                public static SettingDescriptor PriceEvaluationCacheRowLimit { get; } = new SettingDescriptor
                {
                    Name = "Pricing.Evaluation.Cache.RowLimit",
                    GroupName = "Pricing|General",
                    ValueType = SettingValueType.PositiveInteger,
                    DefaultValue = 100000,
                };

                public static SettingDescriptor PriceEvaluationCacheTtl { get; } = new SettingDescriptor
                {
                    Name = "Pricing.Evaluation.Cache.Ttl",
                    GroupName = "Pricing|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = "00:15:00",
                };

                public static SettingDescriptor LogPricingChanges { get; } = new SettingDescriptor
                {
                    Name = "Pricing.LogPricingChanges",
                    GroupName = "Pricing|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                };

                public static SettingDescriptor PriceIndexingValue { get; } = new SettingDescriptor
                {
                    Name = "Pricing.Indexing.PriceIndexingValue",
                    GroupName = "Pricing|General",
                    ValueType = SettingValueType.ShortText,
                    DefaultValue = PriceIndexingValueMax,
                    AllowedValues = new[] { PriceIndexingValueMax, PriceIndexingValueMin }
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                               {
                                   ExportImportPageSize,
                                   PriceEvaluationCacheEnabled,
                                   PriceEvaluationCacheRowLimit,
                                   PriceEvaluationCacheTtl,
                                   LogPricingChanges,
                                   IndexationDatePricingCalendar,
                                   PricingIndexing,
                                   EventBasedIndexation,
                                   PriceIndexingValue,
                               };
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings { get; } = General.AllSettings;
        }
    }
}
