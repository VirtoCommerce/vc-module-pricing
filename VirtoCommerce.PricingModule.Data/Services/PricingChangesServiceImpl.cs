using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Repositories;
using coreModel = VirtoCommerce.Domain.Pricing.Model;
using dataModel = VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingChangesServiceImpl : ServiceBase, IPricingChangesService
    {
        private readonly Func<IPricingRepository> _repositoryFactory;

        public PricingChangesServiceImpl(Func<IPricingRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        #region IPricingChangesService Members

        public virtual IEnumerable<coreModel.PriceCalendarChange> GetCalendarChanges(
            DateTime? lastEvaluationTimestamp = null, DateTime? evaluationTimestamp = null,
            int? skip = null, int? take = null)
        {
            coreModel.PriceCalendarChange[] page = null;

            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                // Increase command timeout to allow lengthy queries when pagination is disabled.
                if (page == null)
                    repository.SetCommandTimeout(TimeSpan.MaxValue);

                lastEvaluationTimestamp = lastEvaluationTimestamp ?? DateTime.MinValue;
                evaluationTimestamp = evaluationTimestamp ?? DateTime.UtcNow;

                var query = repository.Prices
                    .Where(x => (x.EndDate < evaluationTimestamp && x.EndDate > lastEvaluationTimestamp)
                                || (x.StartDate <= evaluationTimestamp && x.StartDate > lastEvaluationTimestamp))
                    .OrderBy(x => x.ProductId) as IQueryable<dataModel.PriceEntity>;

                if (skip != null)
                    query = query.Skip(skip.Value);
                if (take != null)
                    query = query.Take(take.Value);

                var groupedQuery = query
                    .Select(x => x.ProductId)
                    .GroupBy(x => x);

                var changes = groupedQuery
                    .AsNoTracking()
                    .Select(x => new coreModel.PriceCalendarChange
                    {
                        ProductId = x.Key
                    });

                // Stream results when there's no pagination.
                if (take == null)
                {
                    foreach (var calendarChange in changes)
                    {
                        yield return calendarChange;
                    }
                }
                else
                {
                    page = changes.ToArray();
                }
            }

            // Return page without keeping the repository open, so that we optimize resource utilization.
            if (page != null)
            {
                foreach (var calendarChange in page)
                {
                    yield return calendarChange;
                }
            }
        }

        #endregion
    }
}
