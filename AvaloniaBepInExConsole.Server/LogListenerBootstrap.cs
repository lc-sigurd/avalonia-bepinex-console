using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using JetBrains.Annotations;
using Sigurd.AvaloniaBepInExConsole.LogService;

namespace Sigurd.AvaloniaBepInExConsole;

internal static class LogListenerBootstrap
{
    private static ILogMessageQueue? _queue;
    private static LogQueueProcessor? _processor;
    private static AvaloniaLogListener? _listener;

    [UsedImplicitly]
    static void Start()
    {
        _queue = new DefaultLogMessageQueue(32);
        _processor = new LogQueueProcessor(_queue);
        _listener = new AvaloniaLogListener(_queue);

        Logger.Listeners.Add(_listener);

        var cts = new CancellationTokenSource();
        Task.Run(() => _processor.StartAsync(cts.Token), cts.Token);
    }
}
