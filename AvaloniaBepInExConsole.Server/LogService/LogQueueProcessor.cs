using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using Microsoft.Extensions.Hosting;
using NetMQ;
using NetMQ.Sockets;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

public class LogQueueProcessor(ILogMessageQueue logQueue, ManualLogSource logger) : BackgroundService
{
    private PublisherSocket? _publisherSocket;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _publisherSocket = new PublisherSocket(">tcp://localhost:38554");
        // https://github.com/zeromq/netmq/issues/482
        await Task.Delay(500, stoppingToken);
        await ProcessLogQueueAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (SocketAlive) {
            _publisherSocket.Dispose();
            _publisherSocket = null;
        }
    }

    private async Task ProcessLogQueueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested) {
            try {
                var logEventArgs = await logQueue.DequeueAsync(cancellationToken);
                if (logEventArgs.Source == logger)
                    continue;
                PublishLogMessage(logEventArgs);
            }
            catch (OperationCanceledException) { }
            catch (Exception exc) {
                logger.LogError($"Encountered exception while trying to publish a message\n{exc}");
            }
        }
    }

    private void PublishLogMessage(LogEventArgs logEventArgs)
    {
        EnsureSocketAlive();
#if DEBUG
        logger.LogDebug($"Publishing message: {logEventArgs}");
#endif
        _publisherSocket.TrySendFrame(logEventArgs.ToString(), logQueue.HasQueuedLogMessages);
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
