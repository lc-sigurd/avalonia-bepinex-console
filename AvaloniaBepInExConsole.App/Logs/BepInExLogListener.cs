using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Adaptive.Aeron;
using Adaptive.Aeron.LogBuffer;
using Adaptive.Agrona;
using DynamicData;
using Microsoft.Extensions.Hosting;
using OdinSerializer;
using Sigurd.AvaloniaBepInExConsole.Common;
using Sigurd.AvaloniaBepInExConsole.Common.Concurrent;
using Sigurd.AvaloniaBepInExConsole.Common.Extensions;

namespace Sigurd.AvaloniaBepInExConsole.App.Logs;

public class BepInExLogListener : BackgroundService, ILogListener
{
    private const string Channel = "aeron:ipc?term-length=128k";
    private const int StreamId = 0x73cfd0;

    public SourceList<LogEvent> LogMessages { get; } = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var handler = HandlerHelper.ToFragmentHandler(HandleAeronIpcMessage);
        var fragmentedHandler = new FragmentAssembler(handler);

        using var aeron = Aeron.Connect();
        using var subscription = aeron.AddSubscription(Channel, StreamId);

        await subscription.PollLoopAsync(fragmentedHandler, 1, new DelayAsyncIdleStrategy(10), stoppingToken);
    }

    unsafe void HandleAeronIpcMessage(IDirectBuffer buffer, int offset, int length, Header header)
    {
        using var stream = new UnmanagedMemoryStream(
            (byte*)(buffer.BufferPointer + offset).ToPointer(),
            length,
            length,
            FileAccess.Read
        );
        var packet = SerializationUtility.DeserializeValue<EventPacket>(stream, DataFormat.Binary);

        switch (packet.EventType) {
            case EventType.GameLifetime:
                ReceiveGameLifetimeEvent((GameLifetimeEvent)packet.EventData);
                break;
            case EventType.Log:
                ReceiveLogMessage((LogEvent)packet.EventData);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void ReceiveGameLifetimeEvent(GameLifetimeEvent gameLifetimeEvent)
    {
        switch (gameLifetimeEvent.Type) {
            case GameLifetimeEventType.Start:
#if DEBUGAPP
                Console.WriteLine("Received start event, clearing log");
#endif
                LogMessages.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void ReceiveLogMessage(LogEvent logEvent)
    {
#if DEBUGAPP
        Console.WriteLine($"Received log message {logEvent}");
#endif
        LogMessages.Add(logEvent);
    }
}
