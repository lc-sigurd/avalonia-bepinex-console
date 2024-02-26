using OdinSerializer;

namespace Sigurd.AvaloniaBepInExConsole.Common;

public record GameLifetimeEvent : IConsoleEvent
{
    [OdinSerialize]
    public required GameLifetimeEventType Type { get; init; }
}
