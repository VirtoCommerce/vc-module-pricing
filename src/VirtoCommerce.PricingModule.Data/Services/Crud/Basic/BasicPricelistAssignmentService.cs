using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services.Crud.Basic
{
    public class BasicPricelistAssignmentService<TModel, TEntity, TChangeEvent, TChangedEvent> : CrudService<TModel, TEntity, TChangeEvent, TChangedEvent>
        where TModel : PricelistAssignment
        where TEntity : PricelistAssignmentEntity, Platform.Core.Domain.IDataEntity<TEntity, TModel>
        where TChangeEvent : GenericChangedEntryEvent<TModel>
        where TChangedEvent : GenericChangedEntryEvent<TModel>
    {
        private readonly AbstractValidator<IEnumerable<TModel>> _validator;

        public BasicPricelistAssignmentService(Func<IPricingRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            AbstractValidator<IEnumerable<TModel>> validator)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _validator = validator;
        }

        protected override async Task<IEnumerable<TEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return (IEnumerable<TEntity>)await ((IPricingRepository)repository).GetPricelistAssignmentsByIdAsync(ids);
        }

        protected override Task BeforeSaveChanges(IEnumerable<TModel> models)
        {
            _validator.ValidateAndThrow(models);

            return base.BeforeSaveChanges(models);
        }

        protected override void ClearCache(IEnumerable<TModel> models)
        {
            foreach (var assignment in models)
            {
                GenericCachingRegion<TModel>.ExpireTokenForKey(assignment.Id);
                GenericCachingRegion<Pricelist>.ExpireTokenForKey(assignment.PricelistId);
            }
            GenericSearchCachingRegion<TModel>.ExpireRegion();
            GenericCachingRegion<TModel>.ExpireRegion();
        }
    }
}
