namespace Sigurd.AvaloniaBepInExConsole.Common;

public enum EventType
{
    GameLifetime = 1,
    Log = 2,
}

public struct EventPacket
{
    private EventPacket(EventType eventType, IConsoleEvent eventData)
    {
        EventType = eventType;
        EventData = eventData;
    }

    public EventType EventType;
    public IConsoleEvent EventData;
}
