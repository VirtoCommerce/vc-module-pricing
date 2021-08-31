using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Caching;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricelistAssignmentService : CrudService<PricelistAssignment, PricelistAssignmentEntity, PricelistAssignmentChangingEvent, PricelistAssignmentChangedEvent>
                                            , IPricelistAssignmentService
    {
        private readonly ILogger<PricelistAssignmentService> _logger;
        public PricelistAssignmentService(Func<IPricelistAssignmentRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher,
            ILogger<PricelistAssignmentService> logger)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _logger = logger;
        }

        protected override Task<IEnumerable<PricelistAssignmentEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return ((IPricelistAssignmentRepository)repository).GetByIdsAsync(ids);
        }

        public virtual async Task<IEnumerable<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(EvaluatePriceListsAsync));
            var priceListAssignments = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(GenericCachingRegion<PricelistAssignment>.CreateChangeToken());

                return await GetAllPricelistAssignments();
            });

            var query = priceListAssignments.AsQueryable();

            if (evalContext.CatalogId != null)
            {
                query = query.Where(x => x.CatalogId == evalContext.CatalogId);
            }

            if (evalContext.Currency != null)
            {
                query = query.Where(x => x.Pricelist.Currency == evalContext.Currency.ToString());
            }

            if (evalContext.CertainDate != null)
            {
                query = query.Where(x => (x.StartDate == null || evalContext.CertainDate >= x.StartDate) && (x.EndDate == null || x.EndDate >= evalContext.CertainDate));
            }

            var assignments = query.ToArray();
            var assignmentsToReturn = assignments.Where(x => x.DynamicExpression == null).ToList();

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

            return assignmentsToReturn.OrderByDescending(x => x.Priority).ThenByDescending(x => x.Name).Select(x => x.Pricelist);
        }

        public virtual async Task<PricelistAssignment[]> GetAllPricelistAssignments()
        {
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                return (await ((IPricelistAssignmentRepository)repository).PricelistAssignments.Include(x => x.Pricelist).AsNoTracking().ToArrayAsync())
                    .Select(x => x.ToModel(AbstractTypeFactory<PricelistAssignment>.TryCreateInstance())).ToArray();
            }
        }

        protected override void ClearCache(IEnumerable<PricelistAssignment> assignments)
        {
            foreach (var assignment in assignments)
            {
                GenericCachingRegion<PricelistAssignment>.ExpireTokenForKey(assignment.Id);
                GenericCachingRegion<Pricelist>.ExpireTokenForKey(assignment.PricelistId);
            }
            GenericSearchCachingRegion<PricelistAssignment>.ExpireRegion();
            GenericCachingRegion<PricelistAssignment>.ExpireRegion();
        }
    }
}
