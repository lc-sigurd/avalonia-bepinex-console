using System.Threading;
using System.Threading.Tasks;
using Adaptive.Aeron;
using Adaptive.Aeron.LogBuffer;
using Sigurd.AvaloniaBepInExConsole.Common.Concurrent;

namespace Sigurd.AvaloniaBepInExConsole.Common.Extensions;

public static class SubscriptionExtensions
{
    public static ValueTask PollLoopAsync(
        this Subscription subscription,
        FragmentHandler fragmentHandler,
        int fragmentLimit,
        IAsyncIdleStrategy asyncIdleStrategy,
        CancellationToken cancellationToken = default
    )
    {
        return PollLoopAsync(
            subscription,
            HandlerHelper.ToFragmentHandler(fragmentHandler),
            fragmentLimit,
            asyncIdleStrategy,
            cancellationToken
        );
    }

    public static async ValueTask PollLoopAsync(
        this Subscription subscription,
        IFragmentHandler fragmentHandler,
        int fragmentLimit,
        IAsyncIdleStrategy asyncIdleStrategy,
        CancellationToken cancellationToken = default
    ) {
        while (true) {
            if (cancellationToken.IsCancellationRequested) return;
            var workCount = subscription.Poll(fragmentHandler, fragmentLimit);
            await asyncIdleStrategy.Idle(workCount);
        }
    }
}
