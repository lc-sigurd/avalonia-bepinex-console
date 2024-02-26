using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using NetMQ;
using Sigurd.AvaloniaBepInExConsole.LogService;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace Sigurd.AvaloniaBepInExConsole;

public sealed class Manager : MonoBehaviour
{
    private ManualLogSource? _logger;
    private ILogMessageQueue? _queue;
    private LogQueueProcessor? _processor;
    private AvaloniaLogListener? _listener;
    private CancellationTokenSource? _cts;

    private void Awake()
    {
        _logger = Logger.CreateLogSource("Avalonia Console Server");
        var internalLogger = Logger.CreateLogSource("Avalonia Console Server/Internal");

        _queue = new DefaultLogMessageQueue(32);
        _processor = new LogQueueProcessor(_queue, internalLogger);
        _listener = new AvaloniaLogListener(_queue, internalLogger);

        Logger.Listeners.Add(_listener);
        _logger.LogInfo("Listener initialised");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        Task.Run(async () => {
            await _processor.StartAsync(_cts.Token);
            await _processor.ExecuteTask!;
            await _processor.StopAsync(default);
        });
        _logger.LogInfo("Processor started");
    }

    private void OnApplicationQuit()
    {
        if (_cts is { IsCancellationRequested: false }) _cts.Cancel();
        NetMQConfig.Cleanup();
    }
}
