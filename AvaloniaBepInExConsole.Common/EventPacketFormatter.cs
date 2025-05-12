using OdinSerializer;
using Sigurd.AvaloniaBepInExConsole.Common;

[assembly: RegisterFormatter(typeof(EventPacketFormatter))]

namespace Sigurd.AvaloniaBepInExConsole.Common;

public class EventPacketFormatter: MinimalBaseFormatter<EventPacket>
{
    private static readonly Serializer<int> IntSerializer = Serializer.Get<int>();
    private static readonly Serializer<GameLifetimeEvent> GameLifetimeEventSerializer = Serializer.Get<GameLifetimeEvent>();
    private static readonly Serializer<LogEvent> LogEventSerializer = Serializer.Get<LogEvent>();

    protected override void Read(ref EventPacket value, IDataReader reader)
    {
        value.EventType = (EventType)IntSerializer.ReadValue(reader);
        value.EventData = value.EventType switch
        {
            EventType.GameLifetime => GameLifetimeEventSerializer.ReadValue(reader),
            EventType.Log => LogEventSerializer.ReadValue(reader),
            _ => throw new System.NotImplementedException()
        };
    }

    protected override void Write(ref EventPacket value, IDataWriter writer)
    {
        IntSerializer.WriteValue((int)value.EventType, writer);
        switch (value.EventType) {
            case EventType.GameLifetime:
                GameLifetimeEventSerializer.WriteValue((GameLifetimeEvent)value.EventData, writer);
                break;
            case EventType.Log:
                LogEventSerializer.WriteValue((LogEvent)value.EventData, writer);
                break;
            default:
                throw new System.NotImplementedException();
        }
    }
}
