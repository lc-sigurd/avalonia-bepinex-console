using System;
using OdinSerializer;
using Sigurd.AvaloniaBepInExConsole.Common.Extensions;

namespace Sigurd.AvaloniaBepInExConsole.Common;

public record LogEvent : IConsoleEvent, IAnsiFormattable
{
    [OdinSerialize]
    public required object Content { get; init; }

    [OdinSerialize]
    public required BepInExLogLevel Level { get; init; }

    [OdinSerialize]
    public required string SourceName { get; init; }

    [OdinSerialize]
    public required long Order { get; init; }

    public override string ToString() => $"{Content}";

    public string ToStringLine() => $"{this}{Environment.NewLine}";

    public string ToAnsiFormattedString() => $"{Level.GetLevelAnsiReset()}{this}\x1b[0m";

    public string AnsiFormattedContent => ToAnsiFormattedString();
}
