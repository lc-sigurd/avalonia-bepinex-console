using Avalonia.Controls;

namespace Sigurd.AvaloniaBepInExConsole.App.Extensions;

internal static class ScrollChangedEventArgsExtensions
{
    public static bool IsScrolledToEnd(this ScrollChangedEventArgs args) => args.OffsetDelta.Y + args.ViewportDelta.Y > args.ExtentDelta.Y - 7.0;
    public static bool DidScrollUp(this ScrollChangedEventArgs args) => args.OffsetDelta.Y < 0;
}
