using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.Search
{
    public class ProductPriceDocumentChangesProvider : IIndexDocumentChangesProvider, IPricingDocumentChangesProvider
    {
        private const string _changeLogObjectType = nameof(Price);
        private static readonly TimeSpan _calendarChangesInterval = TimeSpan.FromDays(1);
        private const int _batchSize = 500;

        private readonly IChangeLogSearchService _changeLogSearchService;
        private readonly ISettingsManager _settingsManager;
        private readonly Func<IPricingRepository> _repositoryFactory;

        public ProductPriceDocumentChangesProvider(IChangeLogSearchService changeLogSearchService, ISettingsManager settingsManager, Func<IPricingRepository> repositoryFactory)
        {
            _changeLogSearchService = changeLogSearchService;
            _settingsManager = settingsManager;
            _repositoryFactory = repositoryFactory;
        }

        public virtual async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
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
                result = (await InnerGetChangesAsync(startDate, endDate)).TotalCount;
            }

            return result;
        }

        public virtual async Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate,
            long skip, long take)
        {
            IList<IndexDocumentChange> result;

            if (startDate == null && endDate == null)
            {
                result = null;
            }
            else
            {
                result = (await InnerGetChangesAsync(startDate, endDate, (int)skip, (int)take)).Results.ToList();
            }
            return result;
        }

        #region IPricingDocumentChangesProvider Members
        public virtual async Task<GenericSearchResult<IndexDocumentChange>> GetCalendarChangesAsync(DateTime? startDate, DateTime? endDate, int skip = 0, int take = 0)
        {
            var result = new GenericSearchResult<IndexDocumentChange>();

            using (var repository = _repositoryFactory())
            {
                //Return calendar changes only for prices that have at least one specific date range.
                var calendarChangesQuery = repository.Prices.Where(x => x.StartDate != null || x.EndDate != null)
                                                            .Where(x => (x.StartDate == null || x.StartDate <= endDate) && (x.EndDate == null || x.EndDate > startDate)).Select(x => x.ProductId).Distinct();

                result.TotalCount += await calendarChangesQuery.CountAsync();
                if (take > 0)
                {
                    result.Results = calendarChangesQuery.OrderBy(x => x).Skip(skip).Take(take)
                                                         .Select(x => new IndexDocumentChange { DocumentId = x, ChangeType = IndexDocumentChangeType.Modified }).ToList();
                }
            }
            return result;
        }
        #endregion

        //Need to return changes are taken from both sources -  the platform operations log and the pricing entries based on calendar dates
        protected virtual async Task<GenericSearchResult<IndexDocumentChange>> InnerGetChangesAsync(DateTime? startDate, DateTime? endDate, int skip = 0, int take = 0)
        {
            var result = new GenericSearchResult<IndexDocumentChange>();
            var workSkip = 0;
            var workTake = 0;

            var changedProductIds = await GetProductIdsForChangedPricesAsync(startDate, endDate);

            result.TotalCount = changedProductIds.Count;
            workSkip = Math.Min(result.TotalCount, skip);
            workTake = Math.Min(take, Math.Max(0, result.TotalCount - skip));

            if (workTake > 0)
            {
                var productIds = changedProductIds.Skip(workSkip).Take(workTake).ToArray();
                result.Results.AddRange(productIds.Select(x => new IndexDocumentChange { DocumentId = x, ChangeType = IndexDocumentChangeType.Modified }));
            }

            //Re-index calendar prices only once per defined time interval
            var lastIndexDate = _settingsManager.GetValue(ModuleConstants.Settings.General.IndexationDatePricingCalendar.Name, (DateTime?)null) ?? DateTime.MinValue;
            if ((DateTime.UtcNow - lastIndexDate) > _calendarChangesInterval && startDate != null && endDate != null)
            {
                workSkip = skip - workSkip;
                workTake = take - workTake;

                var calendarChanges = await GetCalendarChangesAsync(startDate.Value, endDate.Value, workSkip, workTake);
                result.TotalCount += calendarChanges.TotalCount;
                if (workTake > 0)
                {
                    _settingsManager.SetValue(ModuleConstants.Settings.General.IndexationDatePricingCalendar.Name, DateTime.UtcNow);
                    result.Results.AddRange(calendarChanges.Results);
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves all price changes and groups them by the product Id they are associated with.
        /// </summary>
        /// <param name="startDate">The date time period from</param>
        /// <param name="endDate">The date time period to</param>
        /// <returns>The unique list of products identifiers associated with changed prices for passed period</returns>
        protected virtual async Task<ICollection<string>> GetProductIdsForChangedPricesAsync(DateTime? startDate, DateTime? endDate)
        {
            var result = new HashSet<string>();

            var criteria = new ChangeLogSearchCriteria
            {
                ObjectType = _changeLogObjectType,
                StartDate = startDate,
                EndDate = endDate,
                Take = 0
            };

            // Get changes from operation log
            var operations = (await _changeLogSearchService.SearchAsync(criteria)).Results.Select(x => x.ObjectId);

            using (var repository = _repositoryFactory())
            {
                var totalCount = operations.Count();

                for (var i = 0; i < totalCount; i += _batchSize)
                {
                    var changedPriceIds = operations.Skip(i).Take(_batchSize).ToArray();

                    var productIds = await repository.Prices.Where(x => changedPriceIds.Contains(x.Id))
                                                      .Select(x => x.ProductId)
                                                      .Distinct()
                                                      .ToArrayAsync();
                    result.AddRange(productIds);
                }
            }

            return result;
        }
    }
}
