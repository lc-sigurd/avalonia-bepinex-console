using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public interface ILogMessageQueue
{
    ValueTask QueueAsync(LogEventArgs workItem, CancellationToken cancellationToken = default);
    ValueTask<LogEventArgs> DequeueAsync(CancellationToken cancellationToken);
    bool HasQueuedLogMessages { get; }
}
