using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Search
{
    public class ProductPriceDocumentChangesProvider : IPricingDocumentChangesProvider
    {
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly Func<IPlatformRepository> _platformRepositoryFactory;
        private readonly ISettingsManager _settingsManager;
        private const string _changeLogObjectType = nameof(PriceEntity);
        private static TimeSpan _calendarChangesInterval = TimeSpan.FromDays(1);


        public ProductPriceDocumentChangesProvider(Func<IPricingRepository> repositoryFactory, Func<IPlatformRepository> platformRepositoryFactory, ISettingsManager settingsManager)
        {
            _repositoryFactory = repositoryFactory;
            _platformRepositoryFactory = platformRepositoryFactory;
            _settingsManager = settingsManager;
        }

        public virtual Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            long result;

            if (startDate == null && endDate == null)
            {
                // We don't know the total products count
                result = 0L;
            }
            else
            {
                // Get changes count from operation log
                result = InnerSearchChanges(startDate, endDate).TotalCount;
            }

            return Task.FromResult(result);
        }
        public virtual Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            IList<IndexDocumentChange> result;

            if (startDate == null && endDate == null)
            {
                result = null;
            }
            else
            {
                result = InnerSearchChanges(startDate, endDate, (int)skip, (int)take).Results.ToList();
            }

            return Task.FromResult(result);
        }

        protected virtual GenericSearchResult<IndexDocumentChange> InnerSearchChanges(DateTime? startDate, DateTime? endDate, int skip = 0, int take = 0)
        {
            var result = new GenericSearchResult<IndexDocumentChange>();
            var workSkip = 0;
            var workTake = 0;

            using (var platformRepository = _platformRepositoryFactory())
            using (var repository = _repositoryFactory())
            {
                var operationLogChangesQuery = platformRepository.OperationLogs.Where(x => x.ObjectType == _changeLogObjectType && (startDate == null || x.ModifiedDate >= startDate) && (endDate == null || x.ModifiedDate <= endDate))
                                                                 .OrderBy(x => x.ModifiedDate);
                result.TotalCount = operationLogChangesQuery.Count();
                workSkip = Math.Min(result.TotalCount, skip);
                workTake = Math.Min(take, Math.Max(0, result.TotalCount - skip));
                if (workTake > 0)
                {
                    var changedPriceEntriesIds = operationLogChangesQuery.Skip(workSkip).Take(workTake).Select(x => x.ObjectId).ToArray();
                    result.Results.AddRange(repository.GetPricesByIds(changedPriceEntriesIds).Select(x => new IndexDocumentChange { DocumentId = x.ProductId, ChangeType = IndexDocumentChangeType.Modified }));
                }
                //Re-index calendar prices only once in defined time interval
                var lastIndexDate = _settingsManager.GetValue("VirtoCommerce.Search.IndexingJobs.IndexationDate.Pricing.Calendar", (DateTime?)null) ?? DateTime.MinValue;
                if ((DateTime.UtcNow - lastIndexDate) > _calendarChangesInterval && startDate != null && endDate != null)
                {
                    workSkip = skip - workSkip;
                    workTake = take - workTake;

                    var calendarChangesQuery = repository.Prices.Where(x => (x.StartDate <= endDate) && (x.EndDate >= startDate)).Select(x => x.ProductId).Distinct();
                    result.TotalCount += calendarChangesQuery.Count();
                    if (workTake > 0)
                    {
                        _settingsManager.SetValue("VirtoCommerce.Search.IndexingJobs.IndexationDate.Pricing.Calendar", DateTime.UtcNow);
                        result.Results.AddRange(calendarChangesQuery.OrderBy(x => x).Skip(workSkip).Take(workTake).Select(x => new IndexDocumentChange { DocumentId = x, ChangeType = IndexDocumentChangeType.Modified }).ToArray());
                    }
                }
            }
            return result;
        }
    }
}
