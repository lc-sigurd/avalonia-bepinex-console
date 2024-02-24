using BepInEx.Logging;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public class AvaloniaLogListener : ILogListener
{
    private ILogMessageQueue _taskQueue;
    private ManualLogSource _logger;

    public AvaloniaLogListener(ILogMessageQueue taskQueue, ManualLogSource logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        if (eventArgs.Source == _logger)
            return;
#if DEBUG
        _logger.LogDebug($"Queueing message: {eventArgs}");
#endif
        _taskQueue.QueueAsync(eventArgs)
            .GetAwaiter()
            .GetResult();
    }

    public void Dispose() { }
}
