using BepInEx.Logging;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public class AvaloniaLogListener : ILogListener
{
    private ILogMessageQueue _taskQueue;

    public AvaloniaLogListener(ILogMessageQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        _taskQueue.QueueAsync(eventArgs)
            .GetAwaiter()
            .GetResult();
    }

    public void Dispose() { }
}
