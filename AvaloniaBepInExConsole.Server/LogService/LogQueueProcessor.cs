using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using OdinSerializer;
using Sigurd.AvaloniaBepInExConsole.Common;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public class LogQueueProcessor(ILogMessageQueue logQueue, ManualLogSource logger) : BackgroundService
{
    private PublisherSocket? _publisherSocket;

    protected override async UniTask ExecuteAsync(CancellationToken stoppingToken)
    {
        _publisherSocket = new PublisherSocket(">tcp://localhost:38554");
        // https://github.com/zeromq/netmq/issues/482
        await Task.Delay(500, stoppingToken);
        await ProcessLogQueueAsync(stoppingToken);
    }

    public override async UniTask StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (SocketAlive) {
            _publisherSocket.Dispose();
            _publisherSocket = null;
        }
    }

    private async UniTask ProcessLogQueueAsync(CancellationToken cancellationToken)
    {
        logger.LogInfo("Entering queue processing loop");
        while (!cancellationToken.IsCancellationRequested) {
            try {
                logger.LogDebug("Waiting for event to dequeue");
                var consoleEvent = await logQueue.DequeueAsync(cancellationToken);
                logger.LogDebug("Event dequeued");

                switch (consoleEvent) {
                    case LogEvent logEvent:
                        PublishLogMessage(logEvent);
                        break;
                    case GameLifetimeEvent gameLifetimeEvent:
                        PublishGameLifetimeMessage(gameLifetimeEvent);
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

    private void PublishLogMessage(LogEvent logEvent)
    {
        EnsureSocketAlive();
#if DEBUG
        logger.LogDebug($"Publishing message: {logEvent}");
#endif
        var serializedLogEvent = SerializationUtility.SerializeValue(logEvent, DataFormat.Binary);
        _publisherSocket.SendMoreFrame("logMessage").SendFrame(serializedLogEvent);
#if DEBUG
        logger.LogDebug("Published message");
#endif
    }

    private void PublishGameLifetimeMessage(GameLifetimeEvent gameLifetimeEvent)
    {
        EnsureSocketAlive();
#if DEBUG
        logger.LogDebug($"Publishing game lifetime event: {gameLifetimeEvent}");
#endif
        var serializedGameLifetimeEvent = SerializationUtility.SerializeValue(gameLifetimeEvent, DataFormat.Binary);
        _publisherSocket.SendMoreFrame("gameLifetime").SendFrame(serializedGameLifetimeEvent);
#if DEBUG
        logger.LogDebug("Published game lifetime event");
#endif
    }

    [MemberNotNullWhen(true, nameof(_publisherSocket))]
    private bool SocketAlive => _publisherSocket is { IsDisposed: false };

    [MemberNotNull(nameof(_publisherSocket))]
    private void EnsureSocketAlive()
    {
        if (SocketAlive) return;

        throw new InvalidOperationException("Publisher socket is uninitialized or has been disposed.");
    }
}
