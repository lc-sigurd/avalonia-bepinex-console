using System;
using System.Collections;
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
    private const char Newline = '\n';

    private static bool IsAnsiEsc(char c) => c == AnsiEsc;
    private static bool IsOpenBracket(char c) => c == OpenBracket;
    private static bool IsGraphicsCommand(char c) => c == GraphicsCommand;
    private static bool IsNewline(char c) => c == Newline;

    private readonly string _text;

    private readonly StringBuilder _runTextBuilder;
    private StringBuilder? _escapeBuilder;

    private int _currentIndex;
    private BitArray _omittedCharacterMap;
    private ParseState _currentState;
    private readonly AnsiTextRunPropertiesFactory _propertiesFactory;

    public AnsiFormattedTextSource(string text, TextRunProperties defaultProperties)
    {
        _text = text;
        _omittedCharacterMap = new BitArray(_text.Length);
        _runTextBuilder = new();
        _propertiesFactory = new AnsiTextRunPropertiesFactory(defaultProperties);
    }

    public TextRun? GetTextRun(int textSourceIndex)
    {
        if (textSourceIndex > _text.Length)
            return new TextEndOfParagraph();

        _currentIndex = OutputToSourceIndex(textSourceIndex);
        _currentState = ParseState.Default;
        _runTextBuilder.Clear();
        _escapeBuilder?.Clear();

        while (_currentIndex < _text.Length) {
            if (IsAnsiEsc(Current) && IsOpenBracket(Next)) {
                if (InParseState(ParseState.InAnsiEscape)) throw new AnsiEscapeSyntaxException("Can't ANSI escape inside ANSI escape");
                _currentState |= ParseState.InAnsiEscape;
                _escapeBuilder ??= new StringBuilder();

                _omittedCharacterMap.Set(_currentIndex, true);
                _omittedCharacterMap.Set(_currentIndex + 1, true);
                Advance(2);
                continue;
            }

            if (InParseState(ParseState.InAnsiEscape) && char.IsAsciiLetter(Current)) {
                _omittedCharacterMap.Set(_currentIndex, true);

                if (IsGraphicsCommand(Current) && RunIsValid) {
                    return new TextCharacters(_runTextBuilder.ToString(), _propertiesFactory.BuildProperties());
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
                _omittedCharacterMap.Set(_currentIndex, true);
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

        if (RunIsValid)
            return new TextCharacters(_runTextBuilder.ToString(), _propertiesFactory.BuildProperties());

        return new TextEndOfParagraph();
    }

    private void Advance(int positions = 1) => _currentIndex += positions;
    private char Current => _text[_currentIndex];
    private char Next => _text[_currentIndex + 1];
    private bool InParseState(ParseState state) => (_currentState & state) == state;
    private bool RunIsValid => _runTextBuilder.Length > 0;

    private int OutputToSourceIndex(int outputIndex)
    {
        int sourceIndex = 0;
        int characterCount = 0;
        while (sourceIndex < _text.Length) {
            if (!_omittedCharacterMap[sourceIndex]) characterCount += 1;
            if (characterCount > outputIndex) break;
            sourceIndex++;
        }

        while (sourceIndex > 0 && _omittedCharacterMap[sourceIndex - 1]) {
            sourceIndex--;
        }

        return sourceIndex;
    }

    [Flags]
    private enum ParseState
    {
        Default = 0,
        InMainBody = 1,
        InAnsiEscape = 1 << 1,
    }
}
