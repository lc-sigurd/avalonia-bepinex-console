using OdinSerializer;

namespace Sigurd.AvaloniaBepInExConsole.Common;

public class GameLifetimeEvent : IConsoleEvent
{
    [OdinSerialize]
    public required GameLifetimeEventType Type { get; init; }
}
