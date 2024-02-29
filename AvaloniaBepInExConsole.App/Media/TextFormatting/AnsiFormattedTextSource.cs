using System;
using System.Linq;
using System.Text;
using Avalonia.Media.TextFormatting;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public struct AnsiFormattedTextSource : ITextSource
{
    private const char AnsiEsc = '\x1b';
    private const char AnsiArgDelimiter = ';';
    private const char OpenBracket = '[';
    private const char GraphicsCommand = 'm';

    private static bool IsAnsiEsc(char c) => c == AnsiEsc;
    private static bool IsOpenBracket(char c) => c == OpenBracket;
    private static bool IsGraphicsCommand(char c) => c == GraphicsCommand;

    private readonly string _text;

    private readonly StringBuilder _runTextBuilder;
    private StringBuilder? _escapeBuilder;

    private int _currentIndex;
    private ParseState _currentState;
    private readonly AnsiTextRunPropertiesFactory _propertiesFactory;

    public AnsiFormattedTextSource(string text, TextRunProperties defaultProperties)
    {
        _text = text;
        _runTextBuilder = new();
        _propertiesFactory = new AnsiTextRunPropertiesFactory(defaultProperties);
    }

    public TextRun? GetTextRun(int textSourceIndex)
    {
        if (textSourceIndex > _text.Length)
            return new TextEndOfParagraph();

        _currentIndex = textSourceIndex;
        _currentState = ParseState.Default;
        _runTextBuilder.Clear();
        _escapeBuilder?.Clear();

        while (_currentIndex < _text.Length) {
            if (IsAnsiEsc(Current) && IsOpenBracket(Next)) {
                _currentState |= ParseState.InAnsiEscape;
                _escapeBuilder ??= new StringBuilder();
                Advance(2);
                continue;
            }

            if (InParseState(ParseState.InAnsiEscape) && char.IsAsciiLetter(Current)) {
                if (IsGraphicsCommand(Current) && RunIsValid) {
                    return new AnsiTextCharacters(_runTextBuilder.ToString(), _propertiesFactory.BuildProperties()) {
                        SourceLength = _currentIndex - (_escapeBuilder!.Length + 2) - textSourceIndex,
                    };
                }

                if (IsGraphicsCommand(Current)) {
                    var escapeParameter = _escapeBuilder!.ToString();
                    var parameterValues = escapeParameter
                        .Split(AnsiArgDelimiter)
                        .Select(Int32.Parse);
                    if (_escapeBuilder.Length == 0) {
                        parameterValues = [0];
                    }
                    AnsiGraphics.ApplyGraphicsModes(parameterValues, _propertiesFactory);
                }

                _escapeBuilder!.Clear();
                _currentState ^= ParseState.InAnsiEscape;
                Advance();
                continue;
            }

            if (InParseState(ParseState.InAnsiEscape)) {
                _escapeBuilder!.Append(Current);
                Advance();
                continue;
            }

            _currentState |= ParseState.InMainBody;
            _runTextBuilder.Append(Current);
            Advance();
        }

        if (InParseState(ParseState.InAnsiEscape)) {
            throw new AnsiEscapeSyntaxException("Unterminated ANSI escape.");
        }

        if (RunIsValid) {
            return new AnsiTextCharacters(_runTextBuilder.ToString(), _propertiesFactory.BuildProperties()) {
                SourceLength = _currentIndex - textSourceIndex,
            };
        }

        return new TextEndOfParagraph();
    }

    private void Advance(int positions = 1) => _currentIndex += positions;
    private char Current => _text[_currentIndex];
    private char Next => _text[_currentIndex + 1];
    private bool InParseState(ParseState state) => (_currentState & state) == state;
    private bool RunIsValid => _runTextBuilder.Length > 0;

    [Flags]
    private enum ParseState
    {
        Default = 0,
        InMainBody = 1,
        InAnsiEscape = 1 << 1,
    }
}
