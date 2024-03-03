using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Sigurd.AvaloniaBepInExConsole.LogService;

/// <summary>
/// Base class for implementing a long running <see cref="T:Microsoft.Extensions.Hosting.IHostedService" />.
/// </summary>
public abstract class BackgroundService : IHostedService, IDisposable
{
    private UniTask? _executeTask;
    private CancellationTokenSource? _stoppingCts;

    /// <summary>Gets the Task that executes the background operation.</summary>
    /// <remarks>
    /// Will return <see langword="null" /> if the background operation hasn't started.
    /// </remarks>
    public virtual UniTask? ExecuteTask => _executeTask;

    /// <summary>
    /// This method is called when the <see cref="IHostedService" /> starts. The implementation should return a task that represents
    /// the lifetime of the long running operation(s) being performed.
    /// </summary>
    /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
    /// <remarks>See <see href="https://docs.microsoft.com/dotnet/core/extensions/workers">Worker Services in .NET</see> for implementation guidelines.</remarks>
    protected abstract UniTask ExecuteAsync(CancellationToken stoppingToken);

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous Start operation.</returns>
    public virtual UniTask StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executeTask = ExecuteAsync(_stoppingCts.Token);
        return _executeTask is { Status: UniTaskStatus.Succeeded } ? _executeTask.Value : UniTask.CompletedTask;
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous Stop operation.</returns>
    public virtual async UniTask StopAsync(CancellationToken cancellationToken)
    {
        if (_executeTask == null)
            return;
        try {
            _stoppingCts?.Cancel();
        }
        finally {
            UniTaskCompletionSource<object> state = new UniTaskCompletionSource<object>();
            CancellationTokenRegistration registration = cancellationToken.Register(
                s => ((TaskCompletionSource<object>)s).SetCanceled(),
                state
            );

            try {
                _ = await UniTask.WhenAny(_executeTask ?? UniTask.CompletedTask, state.Task);
            }
            finally {
                await registration.DisposeAsync();
            }
        }
    }

    /// <inheritdoc />
    public virtual void Dispose() => _stoppingCts?.Cancel();
}
