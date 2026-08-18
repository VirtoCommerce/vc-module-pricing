using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Data.Jobs;

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

                var payload = AbstractTypeFactory<LogEntityChangesJobPayload>.TryCreateInstance();
                payload.OperationLogs = logOperations;

                // Background task is used here for performance reasons.
                // The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from
                // the root provider and holds it for the process lifetime, so it must not capture a Scoped dependency.
                // Enqueued even for an empty batch, on purpose: SaveChangesAsync ends in Reset(), and the else branch
                // below shows that resetting the last-modified date on every price change is the intended behavior.
                await BackgroundJob.Enqueue<LogEntityChangesJobHandler>(payload);
            }
            else
            {
                // Force reset the date of last data modifications, so that it would be reset even if the Pricing.LogPricingChanges setting is inactive.
                _lastModifiedDateTime.Reset();
            }
        }

        /// <summary>
        /// Kept for background jobs enqueued by an earlier version, which reference this method by name.
        /// New work goes through <see cref="LogEntityChangesJobHandler"/>; remove this once no such job
        /// can still be pending.
        /// </summary>
        // Signature is byte-identical on purpose: Hangfire persists a queued job as type name + method name +
        // parameter types + serialized args, so changing any of them would strand already-queued entries as Failed.
        // (!) Do not make this method async, it causes improper user recorded into the log! It happens because the user stored in the current thread. If the thread switched, the user info will lost..
        [Obsolete("Enqueued indirectly by legacy Hangfire jobs only; new work uses LogEntityChangesJobHandler.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public void LogEntityChangesInBackground(OperationLog[] operationLogs)
        {
            _changeLogService.SaveChangesAsync(operationLogs).GetAwaiter().GetResult();
        }
    }
}
