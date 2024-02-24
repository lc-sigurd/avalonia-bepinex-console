using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public class DefaultLogMessageQueue : ILogMessageQueue
{
    private readonly Channel<LogEventArgs> _queue;

    public DefaultLogMessageQueue(int capacity)
    {
        BoundedChannelOptions options = new(capacity) {
            FullMode = BoundedChannelFullMode.Wait,
        };

        _queue = Channel.CreateBounded<LogEventArgs>(options);
    }

    public async ValueTask QueueAsync(LogEventArgs workItem, CancellationToken cancellationToken = default)
    {
        if (workItem is null) throw new ArgumentNullException(nameof(workItem));
        await _queue.Writer.WriteAsync(workItem, cancellationToken);
    }

    public async ValueTask<LogEventArgs> DequeueAsync(CancellationToken cancellationToken)
        => await _queue.Reader.ReadAsync(cancellationToken);

    public bool HasQueuedLogMessages => _queue.Reader.Count > 0;
}
