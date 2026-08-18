using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.PricingModule.Data.Jobs
{
    /// <summary>
    /// Persists the change-log entries collected by <see cref="Handlers.LogChangesChangedEventHandler"/>, off the request thread.
    /// </summary>
    /// <remarks>
    /// Awaiting here is safe, unlike in the Hangfire method this replaces: the engine restores the enqueuing user into
    /// the job's scope before Execute, and <c>IUserNameResolver</c> holds it in an AsyncLocal, which flows across
    /// awaits - so <c>CreatedBy</c> on the written rows is the user who made the change, not the thread that resumed.
    /// </remarks>
    public class LogEntityChangesJobHandler(IChangeLogService changeLogService) : IBackgroundJobHandler<LogEntityChangesJobPayload>
    {
        public virtual Task Execute(LogEntityChangesJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return changeLogService.SaveChangesAsync(payload.OperationLogs);
        }
    }
}
