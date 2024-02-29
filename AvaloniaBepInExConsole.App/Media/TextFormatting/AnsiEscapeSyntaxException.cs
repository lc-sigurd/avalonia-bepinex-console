using System;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public class AnsiEscapeSyntaxException : ArgumentException
{
    public AnsiEscapeSyntaxException() { }

    public AnsiEscapeSyntaxException(string message) : base(message) { }

    public AnsiEscapeSyntaxException(string message, Exception inner) : base(message, inner) { }
}
