namespace Sigurd.AvaloniaBepInExConsole.Common;

public enum EventType
{
    GameLifetime = 1,
    Log = 2,
}

public struct EventPacket
{
    public static EventPacket Create(LogEvent logEvent)
    {
        return new EventPacket(EventType.Log, logEvent);
    }

    public static EventPacket Create(GameLifetimeEvent gameLifetimeEvent)
    {
        return new EventPacket(EventType.GameLifetime, gameLifetimeEvent);
    }

    private EventPacket(EventType eventType, IConsoleEvent eventData)
    {
        EventType = eventType;
        EventData = eventData;
    }

    public EventType EventType;
    public IConsoleEvent EventData;
}
