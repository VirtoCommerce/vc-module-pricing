using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.PricingModule.Core
{
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
                public static SettingDescriptor ExportImportPageSize { get; } = new SettingDescriptor
                {
                    Name = "Pricing.ExportImport.PageSize",
                    GroupName = "Pricing|General",
                    ValueType = SettingValueType.Integer,
                    DefaultValue = 50
                };

                public static SettingDescriptor IndexationDatePricingCalendar { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Search.IndexingJobs.IndexationDate.Pricing.Calendar",
                    GroupName = "Pricing|Search",
                    ValueType = SettingValueType.DateTime,
                    DefaultValue = default(DateTime)
                };

                public static SettingDescriptor PricingIndexing { get; } = new SettingDescriptor
                {
                    Name = "Pricing.Indexing.Enable",
                    GroupName = "Pricing|Search",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true
                };

                public static SettingDescriptor StorePricesInIndex { get; } = new SettingDescriptor
                {
                    Name = "Pricing.Indexing.StorePricesInIndex",
                    GroupName = "Pricing|Search",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };
                
                public static SettingDescriptor EventBasedIndexation { get; } = new SettingDescriptor
                {
                    Name = "Pricing.Search.EventBasedIndexation.Enable",
                    GroupName = "Pricing|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                               {
                                   ExportImportPageSize,
                                   IndexationDatePricingCalendar,
                                   PricingIndexing,
                                   EventBasedIndexation,
                                   StorePricesInIndex
                               };
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings { get; } = General.AllSettings;
        }
    }
}
