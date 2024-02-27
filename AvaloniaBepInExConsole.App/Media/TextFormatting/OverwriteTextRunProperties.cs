using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public class OverwriteTextRunProperties : TextRunProperties
{
    private readonly TextRunProperties _defaults;

    public OverwriteTextRunProperties(TextRunProperties defaults)
    {
        _defaults = defaults;
    }

    public override Typeface Typeface => TypefaceOverwrite ?? _defaults.Typeface;

    public Typeface? TypefaceOverwrite { get; set; }

    public override double FontRenderingEmSize => FontRenderingEmSizeOverwrite.GetValueOrDefault(_defaults.FontRenderingEmSize);

    public double? FontRenderingEmSizeOverwrite { get; set; }

    public override TextDecorationCollection? TextDecorations => _defaults.TextDecorations;

    public override IBrush? ForegroundBrush => ForegroundBrushOverwrite ?? _defaults.ForegroundBrush;

    public SolidColorBrush? ForegroundBrushOverwrite { get; set; }

    public override IBrush? BackgroundBrush => BackgroundBrushOverwrite ?? _defaults.BackgroundBrush;

    public SolidColorBrush? BackgroundBrushOverwrite { get; set; }

    public override BaselineAlignment BaselineAlignment => _defaults.BaselineAlignment;

    public override CultureInfo? CultureInfo => _defaults.CultureInfo;

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
