using System;
using System.IO;
using Adaptive.Aeron;
using Adaptive.Agrona;
using Adaptive.Agrona.Concurrent;
using OdinSerializer;
using Sigurd.AvaloniaBepInExConsole.Common;
using Sigurd.AvaloniaBepInExConsole.Common.Extensions;

const string channel = "aeron:ipc?term-length=128k";  // https://aeron.io/docs/cookbook-content/aeron-term-length-msg-size/
const int streamId = 0x73cfd0;  // openssl rand -hex 3

using var myBuffer = new UnsafeBuffer(new byte[16384]);
using var aeron = Aeron.Connect();
using var publisher = aeron.AddPublication(channel, streamId);

var myLogEvent = new LogEvent {
    Content = "foo bar baz; hello, world!",
    Level = BepInExLogLevel.Info,
    SourceName = "fake-server-source",
    Order = 0,
};
var length = WriteLogEvent(myLogEvent, myBuffer, 0);
await publisher.OfferAsync(myBuffer, 0, length);
Console.WriteLine("Sent message");

unsafe int WriteLogEvent(LogEvent logEvent, IDirectBuffer buffer, int offset)
{
    using var stream = new UnmanagedMemoryStream(
        (byte*)(buffer.BufferPointer + offset).ToPointer(),
        offset,
        buffer.Capacity,
        FileAccess.ReadWrite
    );
    var packet = EventPacket.Create(logEvent);
    SerializationUtility.SerializeValue(packet, stream, DataFormat.Binary);
    return (int)stream.Position;
}
