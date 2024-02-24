using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using Microsoft.Extensions.Hosting;
using NetMQ;
using NetMQ.Sockets;

namespace Sigurd.AvaloniaBepInExConsole.App.Logs;

public class BepInExLogListener : BackgroundService
{
    public SourceList<LogMessage> LogMessages { get; } = new();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var runtime = new NetMQRuntime();
        runtime.Run(stoppingToken, ReceiveMessagesAsync(stoppingToken));
        return Task.CompletedTask;
    }

    async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Starting subscriber");
        using var subscriber = new SubscriberSocket("@tcp://localhost:38554");

        subscriber.Subscribe("");
        var newLogs = new LinkedList<LogMessage>();

        while (true) {
            try {
                var (message, more) = await subscriber.ReceiveFrameStringAsync(cancellationToken);
                newLogs.AddLast(new LogMessage(message));
                Console.WriteLine(message);

                if (more && newLogs.Count < 100) continue;

                LogMessages.AddRange(newLogs);
                Console.WriteLine(LogMessages.Count);
                newLogs.Clear();
            }
            catch (OperationCanceledException) { }
            catch (Exception exc) {
                Console.WriteLine(exc);
                // todo: log
            }
        }
    }
}
