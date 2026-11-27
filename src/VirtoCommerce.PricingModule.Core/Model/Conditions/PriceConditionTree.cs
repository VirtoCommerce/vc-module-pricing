using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.PricingModule.Core.Model.Conditions
{
    public class PriceConditionTree : BlockConditionAndOr
    {
        public PriceConditionTree()
        {
            All = true;
        }

        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                var result = IsEmptyRecursive(this);
                return result;
            }
        }

        private static bool IsEmptyRecursive(IConditionTree conditionTree)
        {
            if (!(conditionTree is BlockConditionAndOr))
            {
                return false;
            }

            if (conditionTree is BlockConditionAndOr && conditionTree.Children.Count == 0)
            {
                return true;
            }

            foreach (var child in conditionTree.Children)
            {
                if (!IsEmptyRecursive(child))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
