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

    public virtual IBrush? ForegroundBrush { get; set; }

    public virtual IBrush? BackgroundBrush { get; set; }

    public void Reset()
    {
        Typeface = null;
        ForegroundBrush = null;
        BackgroundBrush = null;
    }

    public TextRunProperties BuildProperties()
    {
        return new AnsiTextRunProperties(Defaults, this);
    }
}
