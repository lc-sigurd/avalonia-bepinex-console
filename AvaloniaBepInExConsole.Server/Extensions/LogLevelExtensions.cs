using System;
using BepInEx.Logging;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.Extensions;

public static class LogLevelExtensions
{
    public static BepInExLogLevel ToAvaloniaBepInExConsoleLogLevel(this LogLevel logLevel)
        => logLevel switch {
            LogLevel.Fatal => BepInExLogLevel.Fatal,
            LogLevel.Error => BepInExLogLevel.Error,
            LogLevel.Warning => BepInExLogLevel.Warning,
            LogLevel.Message => BepInExLogLevel.Message,
            LogLevel.Info => BepInExLogLevel.Info,
            LogLevel.Debug => BepInExLogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
}
