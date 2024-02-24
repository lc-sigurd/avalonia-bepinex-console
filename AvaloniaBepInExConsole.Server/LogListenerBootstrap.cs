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
        var logger = Logger.CreateLogSource("Avalonia Console Server");
        var internalLogger = Logger.CreateLogSource("Avalonia Console Server/Internal");

        _queue = new DefaultLogMessageQueue(32);
        _processor = new LogQueueProcessor(_queue, internalLogger);
        _listener = new AvaloniaLogListener(_queue, internalLogger);

        Logger.Listeners.Add(_listener);
        logger.LogInfo("Listener initialised");

        var cts = new CancellationTokenSource();
        Task.Run(() => _processor.StartAsync(cts.Token), cts.Token);
        logger.LogInfo("Processor started");
    }
}
