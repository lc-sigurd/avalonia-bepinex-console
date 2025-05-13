using System.Threading;
using BepInEx.Logging;
using Cysharp.Threading.Tasks;
using Sigurd.AvaloniaBepInExConsole.Common;
using Sigurd.AvaloniaBepInExConsole.LogService;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace Sigurd.AvaloniaBepInExConsole;

public sealed class Manager : MonoBehaviour
{
    private ManualLogSource? _logger;
    private ILogMessageQueue? _queue;
    private IHostedService? _processorService;
    private AvaloniaLogListener? _listener;
    private CancellationTokenSource? _cts;

    private void Awake()
    {
        _logger = Logger.CreateLogSource(ConsoleServerInfo.PRODUCT_NAME);
        var internalLogger = Logger.CreateLogSource($"{ConsoleServerInfo.PRODUCT_NAME}/Internal");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

        _queue = new DefaultLogMessageQueue(32);
        _processorService = new LogQueueProcessor(_queue, internalLogger);
        _listener = new AvaloniaLogListener(_queue, internalLogger, _cts.Token);

        UniTask.RunOnThreadPool(SubmitStartGameEventToQueue, cancellationToken: _cts.Token)
            .Forget(exc => _logger.LogError($"Uncaught exception occurred during submission of the 'start game' event\n{exc}"), false);

        Logger.Listeners.Add(_listener);
        _logger.LogInfo("Listener initialised");

        UniTask.RunOnThreadPool(RunProcessor)
            .Forget(exc => _logger.LogError($"Uncaught exception occurred in the processor thread\n{exc}"), false);

        _logger.LogInfo("Processor start requested");

        async UniTask SubmitStartGameEventToQueue()
        {
            await _queue.QueueAsync(new GameLifetimeEvent { Type = GameLifetimeEventType.Start }, _cts.Token);
        }

        async UniTask RunProcessor()
        {
            await _processorService.StartAsync(_cts.Token);
            await _processorService.StopAsync(default);
        }
    }

    private void OnDestroy()
    {
        if (_cts is { IsCancellationRequested: false }) _cts.Cancel();
    }
}
