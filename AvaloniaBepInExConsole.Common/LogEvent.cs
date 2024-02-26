using System;
using OdinSerializer;

namespace Sigurd.AvaloniaBepInExConsole.Common;

public record LogEvent
{
    [OdinSerialize]
    public required object Data { get; init; }

    [OdinSerialize]
    public required BepInExLogLevel Level { get; init; }

    [OdinSerialize]
    public required string SourceName { get; init; }

    public override string ToString() => $"[{Level} : {SourceName}] {Data}";

    public string ToStringLine() => $"{this}{Environment.NewLine}";
}
