namespace VirtoCommerce.PricingModule.Data.Caching
{
    public static class PriceEvaluationCacheKey
    {
        public static string TokenKey(string pricelistId, string productId)
        {
            // Length-prefixed to avoid delimiter aliasing between unconstrained id strings.
            return $"{pricelistId?.Length ?? -1}:{pricelistId}:{productId}";
        }
    }
}
