using Avalonia.Controls.Primitives;

namespace Sigurd.AvaloniaBepInExConsole.App.Extensions;

public static class ScrollableExtensions
{
    public static bool IsScrolledToEnd(this IScrollable scroll) => scroll.Offset.Y + scroll.Viewport.Height > scroll.Extent.Height - 1.0;
}
