using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public class OverwriteTextRunProperties : TextRunProperties
{
    public required TextRunProperties Defaults { get; init; }

    public OverwriteTextRunProperties() { }

    [SetsRequiredMembers]
    public OverwriteTextRunProperties(TextRunProperties defaults)
    {
        Defaults = defaults;
    }

    public override Typeface Typeface => TypefaceOverwrite ?? Defaults.Typeface;

    public Typeface? TypefaceOverwrite { get; set; }

    public override double FontRenderingEmSize => FontRenderingEmSizeOverwrite.GetValueOrDefault(Defaults.FontRenderingEmSize);

    public double? FontRenderingEmSizeOverwrite { get; set; }

    public override TextDecorationCollection? TextDecorations => Defaults.TextDecorations;

    public override IBrush? ForegroundBrush => ForegroundBrushOverwrite ?? Defaults.ForegroundBrush;

    public SolidColorBrush? ForegroundBrushOverwrite { get; set; }

    public override IBrush? BackgroundBrush => BackgroundBrushOverwrite ?? Defaults.BackgroundBrush;

    public SolidColorBrush? BackgroundBrushOverwrite { get; set; }

    public override BaselineAlignment BaselineAlignment => Defaults.BaselineAlignment;

    public override CultureInfo? CultureInfo => Defaults.CultureInfo;

    public void ResetOverwrites()
    {
        TypefaceOverwrite = null;
        FontRenderingEmSizeOverwrite = null;
        ForegroundBrushOverwrite = null;
        BackgroundBrushOverwrite = null;
    }

    public TextRunProperties Freeze()
    {
        return new GenericTextRunProperties(
            Typeface,
            FontRenderingEmSize,
            TextDecorations,
            ForegroundBrush,
            BackgroundBrush,
            BaselineAlignment,
            CultureInfo
        );
    }
}
