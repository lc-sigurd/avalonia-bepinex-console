using BepInEx.Logging;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.Extensions;

public static class LogEventArgsExtensions
{
    public static LogEvent ToAvaloniaBepInExConsoleLogEvent(this LogEventArgs logEventArgs, long order)
        => new LogEvent() {
            Content = logEventArgs.GetContent(),
            Level = logEventArgs.Level.ToAvaloniaBepInExConsoleLogLevel(),
            SourceName = logEventArgs.Source.SourceName,
            Order = order,
        };

    private static string GetContent(this LogEventArgs logEventArgs)
    {
        if (logEventArgs is IAnsiFormattable ansiFormattable) {
            return ansiFormattable.ToAnsiFormattedString();
        }

        return logEventArgs.ToString();
    }
}
