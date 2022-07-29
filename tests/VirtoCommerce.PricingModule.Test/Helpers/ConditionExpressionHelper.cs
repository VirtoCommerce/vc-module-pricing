using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.CoreModule.Core.Conditions.Browse;
using VirtoCommerce.CoreModule.Core.Conditions.GeoConditions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Conditions;

namespace VirtoCommerce.PricingModule.Test.Helpers
{
    public class ConditionExpressionHelper
    {
        public static async Task<string> ReadTextFromEmbeddedResourceAsync(string filePath)
        {
            var currentAssembly = typeof(ConditionJsonSerializerTests).Assembly;
            var resourcePath = $"{currentAssembly.GetName().Name}.Resources.{filePath}";

            using var resourceStream = currentAssembly.GetManifestResourceStream(resourcePath);
            using var streamReader = new StreamReader(resourceStream);
            using var jsonTextReader = new JsonTextReader(streamReader);

            var jToken = await JToken.ReadFromAsync(jsonTextReader);
            return jToken.ToString(Formatting.None);
        }

        public static void RegisterTypes()
        {
            AbstractTypeFactory<IConditionTree>.RegisterType<PriceConditionTree>();
            AbstractTypeFactory<IConditionTree>.RegisterType<BlockPricingCondition>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionAgeIs>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionGenderIs>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionLanguageIs>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionStoreSearchedPhrase>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionUrlIs>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionGeoCity>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionGeoCountry>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionGeoState>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionGeoTimeZone>();
            AbstractTypeFactory<IConditionTree>.RegisterType<ConditionGeoZipCode>();
            AbstractTypeFactory<IConditionTree>.RegisterType<UserGroupsContainsCondition>();
        }
    }
}
