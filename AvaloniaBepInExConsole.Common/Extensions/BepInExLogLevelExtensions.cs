namespace Sigurd.AvaloniaBepInExConsole.Common.Extensions;

public static class BepInExLogLevelExtensions
{
    public static string GetLevelAnsiReset(this BepInExLogLevel level)
        => level switch {
            BepInExLogLevel.Debug => "\x1b[0;38;5;8m",
            BepInExLogLevel.Info => "\x1b[0;38;5;7m",
            BepInExLogLevel.Message => "\x1b[0;38;5;15m",
            BepInExLogLevel.Warning => "\x1b[0;38;5;11m",
            BepInExLogLevel.Error => "\x1b[0;1;38;5;1m",
            BepInExLogLevel.Fatal => "\x1b[0;1;38;5;9m",
            _ => "\x1b[0m"
        };
}
