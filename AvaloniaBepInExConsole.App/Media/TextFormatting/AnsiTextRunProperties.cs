using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public class AnsiTextRunProperties : TextRunProperties
{
    public TextRunProperties Defaults { get; }

    public AnsiTextRunProperties(TextRunProperties defaults, AnsiTextRunPropertiesFactory factory)
    {
        Defaults = defaults;
        Typeface = factory.TypefaceFactory?.BuildTypeface() ?? defaults.Typeface;
        ForegroundBrush = factory.ForegroundBrush?.BuildBrush() ?? Defaults.ForegroundBrush;
        BackgroundBrush = factory.BackgroundBrush?.BuildBrush() ?? defaults.BackgroundBrush;
    }

    /// <inheritdoc />
    public override Typeface Typeface { get; }

    /// <inheritdoc />
    public override double FontRenderingEmSize => Defaults.FontRenderingEmSize;

    /// <inheritdoc />
    public override TextDecorationCollection? TextDecorations => Defaults.TextDecorations;

    /// <inheritdoc />
    public override IBrush? ForegroundBrush { get; }

    /// <inheritdoc />
    public override IBrush? BackgroundBrush { get; }

    /// <inheritdoc />
    public override BaselineAlignment BaselineAlignment => Defaults.BaselineAlignment;

    /// <inheritdoc />
    public override CultureInfo? CultureInfo => Defaults.CultureInfo;
}
