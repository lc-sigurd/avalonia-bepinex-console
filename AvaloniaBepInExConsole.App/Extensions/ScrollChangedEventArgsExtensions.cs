using Avalonia.Controls;

namespace Sigurd.AvaloniaBepInExConsole.App.Extensions;

internal static class ScrollChangedEventArgsExtensions
{
    public static bool DidScrollUp(this ScrollChangedEventArgs args) => args.OffsetDelta.Y < 0;
}
