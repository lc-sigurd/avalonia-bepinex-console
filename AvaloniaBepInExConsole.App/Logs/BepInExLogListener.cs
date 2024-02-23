using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using NetMQ;
using NetMQ.Sockets;

namespace AvaloniaBepInExConsole.App.Logs;

public class BepInExLogListener : IDisposable
{
    private NetMQPoller? _poller;

    public SourceList<LogMessage> LogMessages { get; } = new();

    public BepInExLogListener()
    {
        new Thread(ReceiveMessages).Start();
    }

    void ReceiveMessages()
    {
        Console.WriteLine("Setting up subscriber");
        using var subscriber = new SubscriberSocket("@tcp://localhost:38554");
        Console.WriteLine("Set up subscriber");
        _poller = new NetMQPoller();
        try {
            _poller.Add(subscriber);
            var newLogs = new LinkedList<LogMessage>();
            int batchIndex;

            subscriber.Subscribe("");
            subscriber.ReceiveReady += (_, args) => {
                newLogs.Clear();

                for (batchIndex = 0; batchIndex < 100; batchIndex++) {
                    if (!args.Socket.TryReceiveFrameString(out var message))
                        break;

                    newLogs.AddLast(new LogMessage(message));
                    Console.WriteLine(message);
                }

                LogMessages.AddRange(newLogs);
                Console.WriteLine(LogMessages.Count);
            };

            Console.WriteLine("Running poller");
            _poller.Run();
        }
        finally {
            Console.WriteLine("Disposing of subscriber");
            _poller.Dispose();
            _poller = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) {
            if (_poller is { IsRunning: true }) _poller.Stop();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
