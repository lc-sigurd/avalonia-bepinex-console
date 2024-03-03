using System.Threading;
using Cysharp.Threading.Tasks;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public interface ILogMessageQueue
{
    UniTask QueueAsync(IConsoleEvent workItem, CancellationToken cancellationToken = default);
    UniTask<IConsoleEvent> DequeueAsync(CancellationToken cancellationToken);
    bool HasQueuedLogMessages { get; }
}
