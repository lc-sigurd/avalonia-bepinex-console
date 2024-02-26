using System.Threading;
using System.Threading.Tasks;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public interface ILogMessageQueue
{
    ValueTask QueueAsync(IConsoleEvent workItem, CancellationToken cancellationToken = default);
    ValueTask<IConsoleEvent> DequeueAsync(CancellationToken cancellationToken);
    bool HasQueuedLogMessages { get; }
}
