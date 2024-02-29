using System;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

namespace Sigurd.AvaloniaBepInExConsole.App.Controls;

public class AnsiFormattedTextBlock : TextBlock
{
    protected override TextLayout CreateTextLayout(string? text)
    {
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        var defaultProperties = new GenericTextRunProperties(
            typeface,
            FontSize,
            TextDecorations,
            Foreground);

        var paragraphProperties = new GenericTextParagraphProperties(FlowDirection, TextAlignment, true, false,
            defaultProperties, TextWrapping, LineHeight, 0, LetterSpacing);

        ITextSource textSource = new AnsiFormattedTextSource(text ?? "", defaultProperties);

        return new TextLayout(
            textSource,
            paragraphProperties,
            TextTrimming,
            maxLines: MaxLines);
    }
}
