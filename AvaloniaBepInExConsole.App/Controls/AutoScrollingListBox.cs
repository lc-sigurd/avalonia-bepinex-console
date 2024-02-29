using System;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace Sigurd.AvaloniaBepInExConsole.App.Controls;

public class AutoScrollingListBox : ListBox
{
    protected new ScrollViewer Scroll = null!;
    protected AutoScrollState CurrentState = AutoScrollState.AutoScrollingToEnd;

    protected override Type StyleKeyOverride => typeof(ListBox);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs args)
    {
        base.OnApplyTemplate(args);
        Scroll = (ScrollViewer)base.Scroll!;
        Scroll.ScrollChanged += OnScrollChanged;
        ((INotifyCollectionChanged)Items.Source).CollectionChanged += OnCollectionChanged;
    }

    protected virtual void OnScrollChanged(object? sender, ScrollChangedEventArgs args)
    {
        bool userScrolledToEnd = args.OffsetDelta.Y + args.ViewportDelta.Y > args.ExtentDelta.Y - 1.0;
        bool userScrolledUp = args.OffsetDelta.Y < 0;

        if (userScrolledUp && !userScrolledToEnd) {
            CurrentState = AutoScrollState.None;
            return;
        }

        if (userScrolledToEnd) {
            CurrentState = AutoScrollState.AutoScrollingToEnd;
        }
    }

    protected virtual void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (CurrentState == AutoScrollState.AutoScrollingToEnd) Dispatcher.UIThread.Post(ScrollToEnd);
    }

    // https://github.com/AvaloniaUI/Avalonia/issues/14365#issuecomment-1914756642
    private void ScrollToEnd()
    {
        Observable.FromEventPattern<EventHandler<ScrollChangedEventArgs>, ScrollChangedEventArgs>(
                handler => Scroll.ScrollChanged += handler,
                handler => Scroll.ScrollChanged -= handler)
            .Take(1)
            .Subscribe(_ =>
                {
                    var isScrolledToEnd = Math.Abs(Scroll.Offset.Y - Scroll.Extent.Height + Scroll.Viewport.Height) == 0;
                    if (isScrolledToEnd) return;
                    ScrollToEnd();
                }
            );
        Scroll.ScrollToEnd();
    }

    /// <summary>
    /// Enumerated type to keep track of the current auto scroll status
    /// </summary>
    public enum AutoScrollState
    {
        None,
        AutoScrollingToEnd,
    }
}
