using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using Sigurd.AvaloniaBepInExConsole.Common;
using Channel = System.Threading.Channels.Channel;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public class DefaultLogMessageQueue : ILogMessageQueue
{
    private readonly System.Threading.Channels.Channel<IConsoleEvent> _queue;

    public DefaultLogMessageQueue(int capacity)
    {
        BoundedChannelOptions options = new(capacity) {
            FullMode = BoundedChannelFullMode.Wait,
        };

        _queue = Channel.CreateBounded<IConsoleEvent>(options);
    }

    public async UniTask QueueAsync(IConsoleEvent workItem, CancellationToken cancellationToken = default)
    {
        if (workItem is null) throw new ArgumentNullException(nameof(workItem));
        await _queue.Writer.WriteAsync(workItem, cancellationToken);
    }

    public async UniTask<IConsoleEvent> DequeueAsync(CancellationToken cancellationToken)
        => await _queue.Reader.ReadAsync(cancellationToken);

    public bool HasQueuedLogMessages => _queue.Reader.Count > 0;
}
