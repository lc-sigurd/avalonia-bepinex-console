using Avalonia.Media;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public class AnsiTypefaceFactory
{
    public Typeface Default { get; }

    public AnsiTypefaceFactory(Typeface @default)
    {
        Default = @default;
    }

    public FontStyle? FontStyle { get; set; }

    public FontWeight? FontWeight { get; set; }

    public bool HasOverwrites {
        get {
            if (FontStyle is not null) return true;
            if (FontWeight is not null) return true;
            return false;
        }
    }

    public Typeface BuildTypeface()
    {
        if (!HasOverwrites) return Default;
        return new Typeface(
            Default.FontFamily,
            FontStyle ?? Default.Style,
            FontWeight ?? Default.Weight,
            Default.Stretch
        );
    }
}
