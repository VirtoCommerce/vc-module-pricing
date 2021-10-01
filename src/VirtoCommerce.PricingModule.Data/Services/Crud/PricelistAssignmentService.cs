using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricelistAssignmentService : CrudService<PricelistAssignment, PricelistAssignmentEntity, PricelistAssignmentChangingEvent, PricelistAssignmentChangedEvent>
    {
        public PricelistAssignmentService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
        }

        protected override async Task<IEnumerable<PricelistAssignmentEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return await ((IPricingRepository)repository).GetPricelistAssignmentsByIdAsync(ids);
        }


        protected override void ClearCache(IEnumerable<PricelistAssignment> models)
        {
            foreach (var assignment in models)
            {
                GenericCachingRegion<PricelistAssignment>.ExpireTokenForKey(assignment.Id);
                GenericCachingRegion<Pricelist>.ExpireTokenForKey(assignment.PricelistId);
            }
            GenericSearchCachingRegion<PricelistAssignment>.ExpireRegion();
            GenericCachingRegion<PricelistAssignment>.ExpireRegion();
        }
    }
}
