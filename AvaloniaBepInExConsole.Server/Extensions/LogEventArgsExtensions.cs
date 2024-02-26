using BepInEx.Logging;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.Extensions;

public static class LogEventArgsExtensions
{
    public static LogEvent ToAvaloniaBepInExConsoleLogEvent(this LogEventArgs logEventArgs)
        => new LogEvent() {
            Data = logEventArgs.Data,
            Level = logEventArgs.Level.ToAvaloniaBepInExConsoleLogLevel(),
            SourceName = logEventArgs.Source.SourceName,
        };
}
