using System.Diagnostics.CodeAnalysis;
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

    public virtual AnsiTypefaceFactory? TypefaceFactory { get; set; }

    public virtual AnsiBrushFactory? ForegroundBrush { get; set; }

    public virtual AnsiBrushFactory? BackgroundBrush { get; set; }

    public void Reset()
    {
        TypefaceFactory = null;
        ForegroundBrush = null;
        BackgroundBrush = null;
    }

    public bool HasOverwrites {
        get {
            if (TypefaceFactory is not null) return true;
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
