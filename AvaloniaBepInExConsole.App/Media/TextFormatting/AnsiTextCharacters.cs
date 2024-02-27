using System;
using Avalonia.Media.TextFormatting;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public class AnsiTextCharacters : TextCharacters
{
    public AnsiTextCharacters(string text, TextRunProperties textRunProperties) : base(text, textRunProperties) { }
    public AnsiTextCharacters(ReadOnlyMemory<char> text, TextRunProperties textRunProperties) : base(text, textRunProperties) { }

    public override int Length => SourceLength;
    public required int SourceLength { get; init; }
}
