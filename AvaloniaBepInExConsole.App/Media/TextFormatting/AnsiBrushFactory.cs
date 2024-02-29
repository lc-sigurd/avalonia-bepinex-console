using Avalonia.Media;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public class AnsiBrushFactory
{
    public IBrush? Default { get; }

    public AnsiBrushFactory(IBrush? @default)
    {
        Default = @default;
    }

    public Color? Color { get; set; }

    public bool HasOverwrites => Color is not null;

    public IBrush? BuildBrush()
    {
        if (!HasOverwrites) return Default;

        return new SolidColorBrush {
            Opacity = Default?.Opacity ?? 1,
            Color = Color!.Value
        };
    }
}
