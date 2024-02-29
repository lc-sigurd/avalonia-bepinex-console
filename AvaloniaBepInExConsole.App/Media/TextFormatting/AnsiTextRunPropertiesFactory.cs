using System.Diagnostics.CodeAnalysis;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public class AnsiTextRunPropertiesFactory
{
    public required TextRunProperties Defaults { get; init; }

    public AnsiTextRunPropertiesFactory() { }

    [SetsRequiredMembers]
    public AnsiTextRunPropertiesFactory(TextRunProperties defaults)
    {
        Defaults = defaults;
    }

    public virtual Typeface? Typeface { get; set; }

    public virtual SolidColorBrush? ForegroundBrush { get; set; }

    public virtual SolidColorBrush? BackgroundBrush { get; set; }

    public void Reset()
    {
        Typeface = null;
        ForegroundBrush = null;
        BackgroundBrush = null;
    }

    public bool HasOverwrites {
        get {
            if (Typeface is not null) return true;
            if (ForegroundBrush is not null) return true;
            if (BackgroundBrush is not null) return true;
            return false;
        }
    }

    public TextRunProperties BuildProperties()
    {
        if (!HasOverwrites) return Defaults;
        return new AnsiTextRunProperties(Defaults, this);
    }
}
