using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Caching;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingEvaluatorService : IPricingEvaluatorService
    {
        private const int _priceQueryChunkSize = 500;
        private static readonly string _priceEvalLoadLockKey = $"{nameof(PricingEvaluatorService)}:LoadPrices";

        private static readonly Meter _meter = new("VirtoCommerce.PricingModule");
        private static readonly Counter<long> _cacheHits = _meter.CreateCounter<long>("pricing.evaluator.cache.hits");
        private static readonly Counter<long> _cacheMisses = _meter.CreateCounter<long>("pricing.evaluator.cache.misses");
        private static readonly Counter<long> _cacheEvictions = _meter.CreateCounter<long>("pricing.evaluator.cache.evictions");
        private static readonly Counter<long> _cacheOversizeRejected = _meter.CreateCounter<long>("pricing.evaluator.cache.oversize_rejected");
        private static readonly Counter<long> _cacheDateBypass = _meter.CreateCounter<long>("pricing.evaluator.cache.date_bypass");

        // Cheap maintained counter backing the size gauge below: incremented on store (only when the
        // entry is actually stored), decremented in the eviction callback by the evicted entry's row
        // count. Process-static like the other instruments (the Meter itself is process-static). It
        // counts cached ROWS: an empty/negative entry charges 1 to SizeLimit (Size = Max(1, Length))
        // but adds 0 here, and eviction of such an entry also subtracts 0 — so the two stay consistent.
        private static long _cachedRowCount;
        private static readonly ObservableGauge<long> _cacheSize =
            _meter.CreateObservableGauge("pricing.evaluator.cache.size", () => Interlocked.Read(ref _cachedRowCount));

        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly ILogger<PricingEvaluatorService> _logger;
        private readonly IPricingPriorityFilterPolicy _pricingPriorityFilterPolicy;
        private readonly IItemService _productService;
        private readonly ISettingsManager _settingsManager;
        private readonly PriceEvaluationCache _priceEvaluationCache;
        private readonly IOptions<CachingOptions> _cachingOptions;

        public PricingEvaluatorService(
                Func<IPricingRepository> repositoryFactory,
                IItemService productService,
                ILogger<PricingEvaluatorService> logger,
                IPlatformMemoryCache platformMemoryCache,
                IPricingPriorityFilterPolicy pricingPriorityFilterPolicy,
                ISettingsManager settingsManager = null,
                PriceEvaluationCache priceEvaluationCache = null,
                IOptions<CachingOptions> cachingOptions = null
            )
        {
            _platformMemoryCache = platformMemoryCache;
            _repositoryFactory = repositoryFactory;
            _logger = logger;
            _pricingPriorityFilterPolicy = pricingPriorityFilterPolicy;
            _productService = productService;
            _settingsManager = settingsManager;
            _priceEvaluationCache = priceEvaluationCache;
            _cachingOptions = cachingOptions;
        }

        protected virtual Task<bool> IsEvaluatorCacheEnabledAsync()
        {
            // The platform-wide master switch does not reach a private MemoryCache on
            // its own (that only happens through PlatformMemoryCache.GetDefaultCacheEntryOptions), so
            // it must be checked explicitly here in addition to the module-level Enabled setting.
            if (_cachingOptions?.Value.CacheEnabled == false)
            {
                return Task.FromResult(false);
            }

            return _settingsManager == null
                ? Task.FromResult(true)
                : _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.PriceEvaluationCacheEnabled);
        }

        public virtual async Task<IList<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext)
        {
            List<PricelistAssignment> assignmentsToReturn;
            var query = await PriceListAssignmentAsync(evalContext);
            if (evalContext.SkipAssignmentValidation)
            {
                // do NOT use ToListAsync as "query" is not EF IAsyncQueryable
                assignmentsToReturn = query.ToList();
            }
            else
            {
                var assignments = query.ToList();
                assignmentsToReturn = assignments.Where(x => x.DynamicExpression == null).ToList();

                foreach (var assignment in assignments.Where(x => x.DynamicExpression != null))
                {
                    try
                    {
                        if (assignment.DynamicExpression.IsSatisfiedBy(evalContext) && assignmentsToReturn.All(x => x.PricelistId != assignment.PricelistId))
                        {
                            assignmentsToReturn.Add(assignment);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to evaluate price assignment condition.");
                    }
                }
            }

            return assignmentsToReturn
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.Name)
                .Select(x =>
                {
                    x.Pricelist.Priority = x.Priority;
                    return x.Pricelist;
                })
                .ToList();
        }

        public virtual async Task<IQueryable<PricelistAssignment>> PriceListAssignmentAsync(PriceEvaluationContext evalContext)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(EvaluatePriceListsAsync));
            var priceListAssignments = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(GenericCachingRegion<PricelistAssignment>.CreateChangeToken());

                return await GetAllPricelistAssignments();
            });

            // Not .AsQueryable().Where(...): that recompiles the expression tree per enumeration, a lock convoy under load.
            IEnumerable<PricelistAssignment> assignments = priceListAssignments;

            if (evalContext.StoreId != null || evalContext.CatalogId != null)
            {
                assignments = assignments.Where(x => MatchesScope(x, evalContext));
            }

            if (evalContext.Currency != null)
            {
                assignments = assignments.Where(x => x.Pricelist.Currency == evalContext.Currency);
            }

            if (evalContext.CertainDate != null)
            {
                assignments = assignments.Where(x => MatchesDate(x, evalContext.CertainDate.Value));
            }

            return assignments.AsQueryable();
        }

        private static bool MatchesScope(PricelistAssignment assignment, PriceEvaluationContext evalContext)
        {
            return (evalContext.StoreId != null && assignment.StoreId == evalContext.StoreId)
                || (evalContext.CatalogId != null && assignment.CatalogId == evalContext.CatalogId);
        }

        private static bool MatchesDate(PricelistAssignment assignment, DateTime certainDate)
        {
            return (assignment.StartDate == null || certainDate >= assignment.StartDate)
                && (assignment.EndDate == null || assignment.EndDate >= certainDate);
        }

        public virtual async Task<PricelistAssignment[]> GetAllPricelistAssignments()
        {
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                return (await repository.PricelistAssignments
                    .Include(x => x.Pricelist).AsSingleQuery().AsNoTracking().ToListAsync())
                    .Select(x => x.ToModel(AbstractTypeFactory<PricelistAssignment>.TryCreateInstance())).ToArray();
            }
        }

        public virtual async Task<IList<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext)
        {
            if (evalContext == null)
            {
                throw new ArgumentNullException(nameof(evalContext));
            }
            if (evalContext.ProductIds == null)
            {
                throw new MissingFieldException("ProductIds");
            }

            if (evalContext.PricelistIds.IsNullOrEmpty())
            {
                evalContext.Pricelists = evalContext.Pricelists.IsNullOrEmpty()
                    ? (await EvaluatePriceListsAsync(evalContext)).ToArray()
                    : evalContext.Pricelists;

                evalContext.PricelistIds = evalContext.Pricelists.Select(x => x.Id).ToArray();
            }

            // Guard on the private eval-row cache, NOT _platformMemoryCache: the latter stays
            // non-null after the storage move (it still backs EvaluatePriceListsAsync), so
            // keying the guard on it would route a null private cache into GetCachedProductPricesAsync
            // and NPE on _priceEvaluationCache.Cache.
            var rawPrices = evalContext.BypassEvaluatorCache || _priceEvaluationCache == null || !await IsEvaluatorCacheEnabledAsync()
                ? await LoadPricesFromDatabaseAsync(evalContext.ProductIds, evalContext.PricelistIds)
                : await GetCachedProductPricesAsync(evalContext.ProductIds, evalContext.PricelistIds, evalContext);
            var prices = ApplyQuantityAndDateFilter(rawPrices, evalContext);

            var result = new List<Price>();
            result.AddRange(await PostProcessPrices(evalContext, prices));

            return result;
        }

        protected virtual async Task<IList<Price>> LoadPricesFromDatabaseAsync(IList<string> productIds, IList<string> pricelistIds, bool fromCurrentDateOnly = false)
        {
            var result = new List<Price>();
            var now = DateTime.UtcNow; // captured once for the whole (possibly chunked) load, not per chunk
            using (var repository = _repositoryFactory())
            {
                foreach (var productIdsChunk in productIds.Chunk(_priceQueryChunkSize))
                {
                    var query = repository.Prices
                        .Include(x => x.Pricelist).AsSingleQuery()
                        .Where(x => productIdsChunk.Contains(x.ProductId))
                        .Where(x => pricelistIds.Contains(x.PricelistId));

                    if (fromCurrentDateOnly)
                    {
                        // Historical collapse: drop rows already expired at load time SQL-side, so the
                        // cached entry never retains historical rows. The bypass path always calls this
                        // with fromCurrentDateOnly: false, so it stays unaffected.
                        query = query.Where(x => x.EndDate == null || x.EndDate > now);
                    }

                    var queryResult = await query.AsNoTracking().ToListAsync();

                    result.AddRange(queryResult.Select(x => x.ToModel(AbstractTypeFactory<Price>.TryCreateInstance())));
                }
            }

            return result;
        }

        protected virtual async Task<IList<Price>> GetCachedProductPricesAsync(IList<string> productIds, IList<string> pricelistIds, PriceEvaluationContext evalContext)
        {
            var fromCurrentDateOnly = _settingsManager?.GetValue<bool>(ModuleConstants.Settings.General.PriceEvaluationCacheFromCurrentDateOnly) ?? false;
            // Captured ONCE and reused as the default effective date below — two independent
            // DateTime.UtcNow calls would make an unset CertainDate always compare as "historical"
            // (the earlier-captured value trailing the later one by a few ticks).
            var now = DateTime.UtcNow;
            var effective = evalContext.CertainDate ?? now;

            var result = new List<Price>();
            var missing = new List<(string PricelistId, string ProductId, string MemKey)>();
            var bypass = new List<(string PricelistId, string ProductId)>();

            foreach (var pricelistId in pricelistIds)
            {
                foreach (var productId in productIds)
                {
                    // Single discriminating segment via the length-prefixed TokenKey — CacheKey.With joins
                    // args with '-', so passing pricelistId/productId separately would let ids containing
                    // '-' alias distinct pairs onto the same memKey.
                    var memKey = CacheKey.Normalize(
                        CacheKey.With(GetType(), nameof(EvaluateProductPricesAsync), PriceEvaluationCacheKey.TokenKey(pricelistId, productId)));

                    if (_priceEvaluationCache.Cache.TryGetValue(memKey, out CachedPriceRows cached))
                    {
                        // A collapsed entry already dropped rows expired before its own LoadTime —
                        // an effective date earlier than that load must not be served from it; fall
                        // through to the bypass bucket instead of serving stale-collapsed rows.
                        if (fromCurrentDateOnly && effective < cached.LoadTime)
                        {
                            bypass.Add((pricelistId, productId));
                        }
                        else
                        {
                            RecordHit();
                            AddClones(result, cached.Rows);             // clone-on-read
                        }
                    }
                    else if (fromCurrentDateOnly && effective < now)
                    {
                        // Cold miss at a historical date: a collapse-populate here would filter out
                        // rows the historical date needs (same rows an entry loaded "now" would drop) —
                        // a cold miss at a historical date must NOT collapse-populate-and-serve.
                        bypass.Add((pricelistId, productId));
                    }
                    else
                    {
                        RecordMiss();
                        missing.Add((pricelistId, productId, memKey));
                    }
                }
            }

            if (bypass.Count > 0)
            {
                _cacheDateBypass.Add(bypass.Count); // one increment per bypassed pair

                // Bypass never touches the collapsed cache — a fresh, unfiltered, request-scoped
                // load only, never stored and never read from a collapsed entry.
                var bypassLoaded = await LoadPricesFromDatabaseAsync(
                    bypass.Select(x => x.ProductId).Distinct().ToArray(),
                    bypass.Select(x => x.PricelistId).Distinct().ToArray(),
                    fromCurrentDateOnly: false);

                var bypassByPair = bypassLoaded
                    .GroupBy(x => (x.PricelistId, x.ProductId))
                    .ToDictionary(g => g.Key, g => g.ToArray());

                foreach (var (pricelistId, productId) in bypass)
                {
                    if (bypassByPair.TryGetValue((pricelistId, productId), out var rows))
                    {
                        result.AddRange(rows);
                    }
                }
            }

            if (missing.Count == 0)
            {
                return result;
            }

            using (await AsyncLock.GetLockByKey(_priceEvalLoadLockKey).LockAsync())
            {
                // Double-check under the single-flight lock.
                var stillMissing = missing.Where(x => !_priceEvaluationCache.Cache.TryGetValue(x.MemKey, out CachedPriceRows _)).ToList();
                // Intentionally does NOT re-apply the `effective < LoadTime` bypass guard here: a
                // pair only reaches `missing` (never `bypass`) when the first-pass routing above already
                // established `effective >= now` under FromCurrentDateOnly (or the mode is off); a
                // concurrent fill under this lock can only push `LoadTime` later than that `now`, never
                // earlier, so `effective >= LoadTime` still holds. For a null `CertainDate`, the later
                // `ApplyQuantityAndDateFilter` re-reads `DateTime.UtcNow` anyway — so completeness holds.
                foreach (var pair in missing.Except(stillMissing))
                {
                    _priceEvaluationCache.Cache.TryGetValue(pair.MemKey, out CachedPriceRows justFilled);
                    AddClones(result, justFilled.Rows);
                }

                if (stillMissing.Count == 0)
                {
                    return result;
                }

                // Capture change tokens BEFORE the DB read, so a concurrent write during the read expires the just-stored entry.
                var tokens = stillMissing
                    .Select(x => PriceEvaluationCacheKey.TokenKey(x.PricelistId, x.ProductId))
                    .Distinct()
                    .ToDictionary(k => k, k => GenericCachingRegion<Price>.CreateChangeTokenForKey(k));

                var loaded = await LoadPricesFromDatabaseAsync(
                    stillMissing.Select(x => x.ProductId).Distinct().ToArray(),
                    stillMissing.Select(x => x.PricelistId).Distinct().ToArray(),
                    fromCurrentDateOnly);

                // Captured AFTER the load completes so LoadTime never precedes the SQL filter's own
                // "now" (captured inside LoadPricesFromDatabaseAsync) — the read-side bypass check
                // (effective < LoadTime) must never under-shoot the actual EndDate cutoff that was applied.
                var loadTime = DateTime.UtcNow;

                var byPair = loaded
                    .GroupBy(x => (x.PricelistId, x.ProductId))
                    .ToDictionary(g => g.Key, g => g.ToArray());

                foreach (var (pricelistId, productId, memKey) in stillMissing)
                {
                    var rows = byPair.TryGetValue((pricelistId, productId), out var found) ? found : Array.Empty<Price>();
                    var tokenKey = PriceEvaluationCacheKey.TokenKey(pricelistId, productId);

                    if (rows.Length > _priceEvaluationCache.RowLimit)
                    {
                        // A single pair's own row count already exceeds the ceiling — MemoryCache
                        // would reject the Set anyway (immediate re-miss on the next eval), so skip the
                        // wasted Set and make the rejection observable. Still return the rows: graceful
                        // degradation, never OOM, never an error.
                        _cacheOversizeRejected.Add(1);
                        AddClones(result, rows);
                        continue;
                    }

                    var stored = _priceEvaluationCache.Cache.GetOrCreateExclusive(memKey, options =>
                    {
                        options.AddExpirationToken(tokens[tokenKey]); // change-token freshness; empty arrays are stored as negative entries
                        options.Size = Math.Max(1, rows.Length); // SizeLimit requires every entry to declare Size
                        options.RegisterPostEvictionCallback((_, value, _, _) =>
                        {
                            _cacheEvictions.Add(1);
                            if (value is CachedPriceRows evicted)
                            {
                                Interlocked.Add(ref _cachedRowCount, -evicted.Rows.Length);
                            }
                        });
                        ApplyCacheEntryExpiration(options);
                        // Count toward the size gauge only when the entry will actually be stored: the
                        // platform skips Set under an ambient CacheDisabler scope, and the eviction
                        // callback (the sole decrement) never fires for an unstored entry, so an
                        // unguarded increment would drift the gauge upward permanently.
                        if (!CacheDisabler.CacheDisabled)
                        {
                            Interlocked.Add(ref _cachedRowCount, rows.Length);
                        }
                        return new CachedPriceRows(rows, loadTime);
                    });

                    AddClones(result, stored.Rows);                   // clone-on-read even for just-stored rows
                }
            }

            return result;
        }

        // Overridable so a consumer can substitute its own expiration policy (e.g. absolute) without
        // reimplementing the evaluator.
        protected virtual void ApplyCacheEntryExpiration(MemoryCacheEntryOptions options)
        {
            options.SlidingExpiration = ParseTtl();
        }

        private TimeSpan ParseTtl()
        {
            // "00:00:00"/negative parse successfully but a non-positive SlidingExpiration throws
            // ArgumentOutOfRangeException at Set — non-positive/invalid Ttl falls back to the default.
            return TimeSpan.TryParse(_settingsManager?.GetValue<string>(ModuleConstants.Settings.General.PriceEvaluationCacheTtl), out var ttl) && ttl > TimeSpan.Zero
                ? ttl
                : TimeSpan.FromMinutes(15);
        }

        // never hand callers the shared singleton-cached instances.
        private static void AddClones(List<Price> target, Price[] cached)
        {
            foreach (var price in cached)
            {
                target.Add(price.CloneTyped());
            }
        }

        private static void RecordHit() => _cacheHits.Add(1);

        private static void RecordMiss() => _cacheMisses.Add(1);

        private static IEnumerable<Price> ApplyQuantityAndDateFilter(IEnumerable<Price> prices, PriceEvaluationContext evalContext)
        {
            // Reproduces the former SQL WHERE (quantity + date window) in memory, so cached rows stay unfiltered.
            var certainDate = evalContext.CertainDate ?? DateTime.UtcNow;

            return prices.Where(x => (evalContext.Quantity >= x.MinQuantity || evalContext.Quantity == 0)
                && (x.StartDate == null || x.StartDate <= certainDate)
                && (x.EndDate == null || x.EndDate > certainDate));
        }

        private async Task<List<Price>> PostProcessPrices(PriceEvaluationContext evalContext, IEnumerable<Price> prices)
        {
            var result = new List<Price>();

            result.AddRange(_pricingPriorityFilterPolicy.FilterPrices(prices, evalContext));

            if (_productService == null)
            {
                return result;
            }
            //Then variation inherited prices
            var productIdsWithoutPrice = evalContext.ProductIds.Except(result.Select(x => x.ProductId).Distinct()).ToArray();
            if (!productIdsWithoutPrice.Any())
            {
                return result;
            }

            //Try to inherit prices for variations from their main product
            //Need find products without price it may be a variation without implicitly price defined and try to get price from main product
            var variations = (await _productService.GetNoCloneAsync(productIdsWithoutPrice, ItemResponseGroup.ItemInfo.ToString()))
                .Where(x => x.MainProductId != null).ToList();
            evalContext = evalContext.CloneTyped();
            evalContext.ProductIds = variations.Select(x => x.MainProductId).Distinct().ToArray();

            if (evalContext.ProductIds.IsNullOrEmpty())
            {
                return result;
            }

            var inheritedPrices = await EvaluateProductPricesAsync(evalContext);
            foreach (var inheritedPrice in inheritedPrices)
            {
                foreach (var variation in variations.Where(x => x.MainProductId == inheritedPrice.ProductId))
                {
                    var variationPrice = inheritedPrice.CloneTyped();
                    //Reset id for correct override price in possible update 
                    variationPrice.Id = null;
                    variationPrice.ProductId = variation.Id;
                    result.Add(variationPrice);
                }
            }

            return result;
        }
    }
}
