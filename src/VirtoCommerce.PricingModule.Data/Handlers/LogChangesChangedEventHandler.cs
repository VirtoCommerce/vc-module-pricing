using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Events;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    public class LogChangesChangedEventHandler : IEventHandler<PriceChangedEvent>
    {
        private readonly IChangeLogService _changeLogService;
        private readonly ILastModifiedDateTime _lastModifiedDateTime;
        private readonly ISettingsManager _settingsManager;

        public LogChangesChangedEventHandler(IChangeLogService changeLogService, ILastModifiedDateTime lastModifiedDateTime, ISettingsManager settingsManager)
        {
            _changeLogService = changeLogService;
            _lastModifiedDateTime = lastModifiedDateTime;
            _settingsManager = settingsManager;
        }

        public virtual Task Handle(PriceChangedEvent message)
        {
            return InnerHandle(message);
        }

        protected virtual async Task InnerHandle<T>(GenericChangedEntryEvent<T> @event) where T : IEntity
        {
            var logPricingChangesEnabled = await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.LogPricingChanges);

            if (logPricingChangesEnabled)
            {
                var logOperations = @event.ChangedEntries.Select(x => AbstractTypeFactory<OperationLog>.TryCreateInstance().FromChangedEntry(x)).ToArray();
                //Background task is used here for performance reasons
                BackgroundJob.Enqueue(() => LogEntityChangesInBackground(logOperations));
            }
            else
            {
                // Force reset the date of last data modifications, so that it would be reset even if the Pricing.LogPricingChanges setting is inactive.
                _lastModifiedDateTime.Reset();
            }
        }

        [DisableConcurrentExecution(10)]
        // "DisableConcurrentExecutionAttribute" prevents to start simultaneous job payloads.
        // Should have short timeout, because this attribute implemented by following manner: newly started job falls into "processing" state immediately.
        // Then it tries to receive job lock during timeout. If the lock received, the job starts payload.
        // When the job is awaiting desired timeout for lock release, it stucks in "processing" anyway. (Therefore, you should not to set long timeouts (like 24*60*60), this will cause a lot of stucked jobs and performance degradation.)
        // Then, if timeout is over and the lock NOT acquired, the job falls into "scheduled" state (this is default fail-retry scenario).
        // Failed job goes to "Failed" state (by default) after retries exhausted.

        // (!) Do not make this method async, it causes improper user recorded into the log! It happens because the user stored in the current thread. If the thread switched, the user info will lost..
        public void LogEntityChangesInBackground(OperationLog[] operationLogs)
        {
            _changeLogService.SaveChangesAsync(operationLogs).GetAwaiter().GetResult();
        }
    }
}
