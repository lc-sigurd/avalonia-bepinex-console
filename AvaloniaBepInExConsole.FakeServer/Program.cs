using System;
using System.IO;
using Adaptive.Aeron;
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
};
var length = WriteLogEvent(myLogEvent, myBuffer.ByteArray, 0);
await publisher.OfferAsync(myBuffer, 0, length);
Console.WriteLine("Sent message");

int WriteLogEvent(LogEvent logEvent, byte[] buffer, int offset)
{
    using var stream = new MemoryStream(buffer, offset, buffer.Length, true);
    var packet = EventPacket.Create(logEvent);
    SerializationUtility.SerializeValue(packet, stream, DataFormat.Binary);
    return (int)stream.Position;
}
