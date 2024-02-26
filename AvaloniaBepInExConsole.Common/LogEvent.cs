using System;
using OdinSerializer;
using Sigurd.AvaloniaBepInExConsole.Common.Extensions;

namespace Sigurd.AvaloniaBepInExConsole.Common;

public record LogEvent : IConsoleEvent
{
    [OdinSerialize]
    public required object Data { get; init; }

    [OdinSerialize]
    public required BepInExLogLevel Level { get; init; }

    [OdinSerialize]
    public required string SourceName { get; init; }

    public override string ToString() => $"[{Level} : {SourceName}] {Data}";

    public string ToStringLine() => $"{this}{Environment.NewLine}";

    public string ToAnsiColouredString() => $"{Level.GetLevelAnsiReset()}{this}\x1b[0m";
}
