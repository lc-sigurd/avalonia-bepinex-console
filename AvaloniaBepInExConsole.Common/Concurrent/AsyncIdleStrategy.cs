using System.Threading.Tasks;

namespace Sigurd.AvaloniaBepInExConsole.Common.Concurrent;

public interface IAsyncIdleStrategy
{
    ValueTask Idle(int workCount);

    ValueTask Idle();

    void Reset();
}

public sealed class DelayAsyncIdleStrategy(int delayPeriodMilliseconds) : IAsyncIdleStrategy
{
    public async ValueTask Idle(int workCount)
    {
        if (workCount > 0) return;
        await Task.Delay(delayPeriodMilliseconds);
    }

    public async ValueTask Idle() => await Task.Delay(delayPeriodMilliseconds);

    public void Reset() { }
}
