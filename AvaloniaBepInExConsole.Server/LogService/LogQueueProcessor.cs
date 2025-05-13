using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Adaptive.Aeron;
using Adaptive.Agrona.Concurrent;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using OdinSerializer;
using Sigurd.AvaloniaBepInExConsole.Common;
using Sigurd.AvaloniaBepInExConsole.Common.Extensions;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public class LogQueueProcessor(ILogMessageQueue logQueue, ManualLogSource logger, ServerConfig config) : BackgroundService
{
    private const string Channel = "aeron:ipc?term-length=128k";  // https://aeron.io/docs/cookbook-content/aeron-term-length-msg-size/
    private const int StreamId = 0x73cfd0;  // openssl rand -hex 3
    private UnsafeBuffer? _buffer;
    private UnmanagedMemoryStream? _stream;
    private Aeron.Context? _aeronContext;
    private Aeron? _aeron;
    private Publication? _publication;

    protected override async UniTask ExecuteAsync(CancellationToken stoppingToken)
    {
        unsafe {
            _buffer = new UnsafeBuffer(new byte[16384]);
            _stream = new UnmanagedMemoryStream(
                (byte*)_buffer.BufferPointer.ToPointer(),
                0,
                _buffer.Capacity,
                FileAccess.ReadWrite
            );
        }

        _aeronContext = new Aeron.Context();
        if (config.AeronDirectoryName is not null) _aeronContext.AeronDirectoryName(config.AeronDirectoryName);
        _aeron = Aeron.Connect(_aeronContext);
        _publication = _aeron.AddPublication(Channel, StreamId);
        await ProcessLogQueueAsync(stoppingToken);
    }

    public override async UniTask StopAsync(CancellationToken cancellationToken)
    {
        if (PublicationAlive) {
            _publication.Dispose();
            _publication = null;

            _aeron!.Dispose();
            _aeron = null;

            _aeronContext!.Dispose();
            _aeronContext = null;

            await _stream!.DisposeAsync();
            _stream = null;

            _buffer!.Dispose();
            _buffer = null;
        }

        await base.StopAsync(cancellationToken);
    }

    private async UniTask ProcessLogQueueAsync(CancellationToken cancellationToken)
    {
        logger.LogInfo("Entering queue processing loop");
        while (!cancellationToken.IsCancellationRequested) {
            try {
#if DEBUG
                logger.LogDebug("Waiting for event to dequeue");
#endif
                var consoleEvent = await logQueue.DequeueAsync(cancellationToken);
#if DEBUG
                logger.LogDebug("Event dequeued");
#endif

                switch (consoleEvent) {
                    case LogEvent logEvent:
                        await PublishLogMessage(logEvent, cancellationToken);
                        break;
                    case GameLifetimeEvent gameLifetimeEvent:
                        await PublishGameLifetimeMessage(gameLifetimeEvent, cancellationToken);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(consoleEvent));
                }
            }
            catch (Exception exc) when (exc is OperationCanceledException or TaskCanceledException) {
                logger.LogInfo("Caught cancellation exception");
            }
            catch (Exception exc) {
                logger.LogError($"Encountered exception while trying to publish a message\n{exc}");
            }
        }
        logger.LogInfo("Exited queue processing loop");
    }

    private async UniTask PublishLogMessage(LogEvent logEvent, CancellationToken cancellationToken = default)
    {
        EnsureSocketAlive();
#if DEBUG
        logger.LogDebug($"Publishing message: {logEvent}");
#endif
        _stream!.Seek(0, SeekOrigin.Begin);
        var packet = EventPacket.Create(logEvent);
        SerializationUtility.SerializeValue(packet, _stream, DataFormat.Binary);
        var length = (int)_stream.Position;
        await _publication.OfferAsync(_buffer!, 0, length, cancellationToken: cancellationToken);
#if DEBUG
        logger.LogDebug("Published message");
#endif
    }

    private async UniTask PublishGameLifetimeMessage(
        GameLifetimeEvent gameLifetimeEvent,
        CancellationToken cancellationToken = default
    ) {
        EnsureSocketAlive();
#if DEBUG
        logger.LogDebug($"Publishing game lifetime event: {gameLifetimeEvent}");
#endif
        _stream!.Seek(0, SeekOrigin.Begin);
        var packet = EventPacket.Create(gameLifetimeEvent);
        SerializationUtility.SerializeValue(packet, _stream, DataFormat.Binary);
        var length = (int)_stream.Position;
        await _publication.OfferAsync(_buffer!, 0, length, cancellationToken: cancellationToken);
#if DEBUG
        logger.LogDebug("Published game lifetime event");
#endif
    }

    [MemberNotNullWhen(true, nameof(_publication))]
    private bool PublicationAlive => _publication is { IsClosed: false };

    [MemberNotNull(nameof(_publication))]
    private void EnsureSocketAlive()
    {
        if (PublicationAlive) return;
        throw new InvalidOperationException("Publication is uninitialized or has been disposed.");
    }
}
