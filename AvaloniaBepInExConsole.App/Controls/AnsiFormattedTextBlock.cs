using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

namespace Sigurd.AvaloniaBepInExConsole.App.Controls;

public class AnsiFormattedTextBlock : TextBlock
{
    private static FieldInfo _constraintFieldInfo = typeof(TextBlock).GetField("_constraint", bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic)!;
    protected override Type StyleKeyOverride => typeof(TextBlock);

    protected Size Constraint => (Size)_constraintFieldInfo.GetValue(this)!;

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
            Constraint.Width,
            Constraint.Height,
            maxLines: MaxLines);
    }
}
