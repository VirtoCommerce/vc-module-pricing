using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.PricingModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that persists price change-log entries.
    /// </summary>
    public class LogEntityChangesJobPayload
    {
        public OperationLog[] OperationLogs { get; set; }
    }
}
