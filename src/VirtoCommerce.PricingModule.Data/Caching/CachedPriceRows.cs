using System;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Caching
{
    // Cached value for a single (pricelistId, productId) entry in PriceEvaluationCache. LoadTime is
    // the moment the rows were loaded (and, in FromCurrentDateOnly mode, the cutoff the SQL-side
    // EndDate filter used) — the read-side bypass compares an evaluation's effective date against it.
    public readonly record struct CachedPriceRows(Price[] Rows, DateTime LoadTime);
}
