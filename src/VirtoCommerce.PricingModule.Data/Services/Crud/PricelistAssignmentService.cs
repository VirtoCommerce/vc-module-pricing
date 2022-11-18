using System;
using System.Collections.Generic;
using FluentValidation;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services.Crud.Basic;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricelistAssignmentService : BasicPricelistAssignmentService<PricelistAssignment, PricelistAssignmentEntity, PricelistAssignmentChangingEvent, PricelistAssignmentChangedEvent>
    {

        public PricelistAssignmentService(Func<IPricingRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            AbstractValidator<IEnumerable<PricelistAssignment>> validator)
            : base(repositoryFactory, platformMemoryCache, eventPublisher, validator)
        {
        }
    }
}
