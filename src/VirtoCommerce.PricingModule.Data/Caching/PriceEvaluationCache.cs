using System;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core;

namespace VirtoCommerce.PricingModule.Data.Caching
{
    // Private, pricing-owned MemoryCache instance (NOT the shared IPlatformMemoryCache): a hard
    // memory ceiling requires MemoryCacheOptions.SizeLimit, which is per-instance and would force
    // every platform cache consumer to declare entry.Size if set on the shared cache.
    public sealed class PriceEvaluationCache : IDisposable
    {
        public IMemoryCache Cache { get; }

        public long RowLimit { get; }

        public PriceEvaluationCache(ISettingsManager settingsManager)
        {
            var configured = settingsManager?.GetValue<int>(
                ModuleConstants.Settings.General.PriceEvaluationCacheRowLimit) ?? 0;
            // Non-positive/invalid/unset falls back to the default, never to a degenerate Max(1, 0) == 1 ceiling.
            RowLimit = configured > 0 ? configured : 100000;
            Cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = RowLimit });
        }

        public void Dispose()
        {
            Cache.Dispose();
        }
    }
}
