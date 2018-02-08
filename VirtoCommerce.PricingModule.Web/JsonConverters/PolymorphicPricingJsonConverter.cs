using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using webModel = VirtoCommerce.PricingModule.Web.Model;

namespace VirtoCommerce.PricingModule.Web.JsonConverters
{
    public class PolymorphicPricingJsonConverter : JsonConverter
    {
        private static Type[] _knowTypes = new[] { typeof(Price), typeof(Pricelist), typeof(PricelistAssignment),
                                                   typeof(PriceEvaluationContext),
                                                   typeof(PricesSearchCriteria), typeof(PricelistAssignmentsSearchCriteria), typeof(PricelistSearchCriteria) };

        public PolymorphicPricingJsonConverter()
        {           
        }

        public override bool CanWrite { get { return false; } }
        public override bool CanRead { get { return true; } }

        public override bool CanConvert(Type objectType)
        {
            return _knowTypes.Any(x => x.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal = null;
            var obj = JObject.Load(reader);

            var tryCreateInstance = typeof(AbstractTypeFactory<>).MakeGenericType(objectType).GetMethods().FirstOrDefault(x => x.Name.EqualsInvariant("TryCreateInstance") && x.GetParameters().Count() == 0);
            retVal = tryCreateInstance.Invoke(null, null);

            serializer.Populate(obj.CreateReader(), retVal);
            return retVal;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}