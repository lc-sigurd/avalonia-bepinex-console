using System;
using System.Threading;
using Adaptive.Agrona.Concurrent;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using Sigurd.AvaloniaBepInExConsole.Extensions;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public class AvaloniaLogListener : ILogListener
{
    private readonly ILogMessageQueue _taskQueue;
    private readonly ManualLogSource _logger;
    private readonly CancellationToken _cancellationToken;
    private readonly AtomicLong _counter = new();

    public AvaloniaLogListener(ILogMessageQueue taskQueue, ManualLogSource logger, CancellationToken token)
    {
        _taskQueue = taskQueue;
        _logger = logger;
        _cancellationToken = token;
    }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (eventArgs.Source == _logger)
            return;

        UniTask.RunOnThreadPool(
            () => SubmitLogEventToQueue(eventArgs, _counter.IncrementAndGet(), _cancellationToken),
            cancellationToken: _cancellationToken
        ).Forget(
            exc => _logger.LogError($"Exception occurred during submission of a log event\n{exc}"),
            false
        );
    }

    private async UniTask SubmitLogEventToQueue(LogEventArgs eventArgs, long order, CancellationToken cancellationToken = default)
    {
#if DEBUG
        _logger.LogDebug($"Queueing message: {eventArgs}");
#endif
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try {
            cts.CancelAfter(5000);
            await _taskQueue.QueueAsync(eventArgs.ToAvaloniaBepInExConsoleLogEvent(order), cts.Token);
        }
        catch (OperationCanceledException exc) {
            _logger.LogError($"Timed out queueing message.\n{exc}");
            return;
        }
#if DEBUG
        _logger.LogDebug("Message queued");
#endif
    }

    public void Dispose() { }
}
