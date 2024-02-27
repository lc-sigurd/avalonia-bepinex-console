using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

namespace Sigurd.AvaloniaBepInExConsole.App.Controls;

public class ANSIFormattedTextBlock : TextBlock
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

        ITextSource textSource = new AnsiTextSource(text, defaultProperties);

        return new TextLayout(
            textSource,
            paragraphProperties,
            TextTrimming,
            maxLines: MaxLines);
    }

    private struct AnsiTextSource : ITextSource
    {
        private const char AnsiEsc = '\x1b';
        private const char AnsiArgDelimiter = ';';
        private const char OpenBracket = '[';
        private const char GraphicsCommand = 'm';

        private static bool IsAnsiEsc(char c) => c == AnsiEsc;
        private static bool IsOpenBracket(char c) => c == OpenBracket;
        private static bool IsGraphicsCommand(char c) => c == GraphicsCommand;

        private delegate void GraphicsModeApplicator(IEnumerator<int> graphicsArgumentEnumerator, AnsiTextSource textSource);

        private static GraphicsModeApplicator ForegroundApplicatorFactory(Color colorToApply)
        {
            return (_, textSource) => {
                textSource._currentProperties.ForegroundBrushOverwrite ??= new SolidColorBrush {
                    Opacity = textSource._currentProperties.ForegroundBrush?.Opacity ?? 1,
                };
                textSource._currentProperties.ForegroundBrushOverwrite.Color = colorToApply;
            };
        }

        private static GraphicsModeApplicator BackgroundApplicatorFactory(Color colorToApply)
        {
            return (_, textSource) => {
                textSource._currentProperties.BackgroundBrushOverwrite ??= new SolidColorBrush {
                    Opacity = textSource._currentProperties.BackgroundBrush?.Opacity ?? 1,
                };
                textSource._currentProperties.BackgroundBrushOverwrite.Color = colorToApply;
            };
        }

        private static Dictionary<int, GraphicsModeApplicator> GraphicsModeApplicators = new() {
            #region basic
            [0] = (enumerator, textSource) => {
                // reset all
                textSource._currentProperties.ResetOverwrites();
            },
            [1] = (enumerator, textSource) => {
                // set bold weight
            },
            [2] = (enumerator, textSource) => {
                // set faint weight
            },
            [22] = (enumerator, textSource) => {
                // reset weight (set regular weight)
            },
            [3]= (enumerator, textSource) => {
                // set italic
            },
            [23] = (enumerator, textSource) => {
                // reset italic
            },
            [4] = (enumerator, textSource) => {
                // set underline
            },
            [24] = (enumerator, textSource) => {
                // reset underline
            },
            [5] = (enumerator, textSource) => {
                // set 'blinking mode'
            },
            [25] = (enumerator, textSource) => {
                // reset blinding mode
            },
            [7] = (enumerator, textSource) => {
                // set inverse/reverse mode
            },
            [27] = (enumerator, textSource) => {
                // reset inverse/reverse mode
            },
            [8] = (enumerator, textSource) => {
                // set hidden/invisible mode
            },
            [28] = (enumerator, textSource) => {
                // reset hidden/invisible mode
            },
            [9] = (enumerator, textSource) => {
                // set strikethrough
            },
            [29] = (enumerator, textSource) => {
                // reset strikethrough
            },
            #endregion

            #region foreground colouring

            #region dark colours
            // Set black
            [30] = ForegroundApplicatorFactory(Colors.Black),
            // Set red
            [31] = ForegroundApplicatorFactory(Colors.DarkRed),
            // Set green
            [32] = ForegroundApplicatorFactory(Colors.DarkGreen),
            // Set yellow
            [33] = ForegroundApplicatorFactory(Colors.Olive),
            // Set blue
            [34] = ForegroundApplicatorFactory(Colors.DarkBlue),
            // Set magenta
            [35] = ForegroundApplicatorFactory(Colors.DarkMagenta),
            // Set cyan
            [36] = ForegroundApplicatorFactory(Colors.DarkCyan),
            // Set white
            [37] = ForegroundApplicatorFactory(Colors.DarkGray),
            #endregion

            [38] = (enumerator, textSource) => {
                switch (GetNext()) {
                    case 2:
                        // set truecolour
                        SetTrueColour();
                        break;
                    case 5:
                        // Set 256-colour
                        Set256Colour();
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }

                void SetRgb(byte r, byte g, byte b)
                {
                    ForegroundApplicatorFactory(Color.FromRgb(r, g, b))(enumerator, textSource);
                }

                void SetTrueColour()
                {
                    var (r, g, b) = (GetNext(), GetNext(), GetNext());
                    SetRgb((byte)r, (byte)g, (byte)b);
                }

                void Set3BitColourDepth(int index)
                {
                    var r = index / 36 * 51;
                    var g = index % 36 / 6 * 51;
                    var b = index % 6 * 51;
                    SetRgb((byte)r, (byte)g, (byte)b);
                }

                void SetGrayscale(int index)
                {
                    var component = (byte)(index * 10 + 8);
                    SetRgb(component, component, component);
                }

                void Set256Colour()
                {
                    var colourId = GetNext();
                    switch (colourId) {
                        case <= 7:
                            GraphicsModeApplicators[30 + colourId](enumerator, textSource);
                            break;
                        case <= 15:
                            GraphicsModeApplicators[82 + colourId](enumerator, textSource);
                            break;
                        case <= 231:
                            Set3BitColourDepth(colourId - 16);
                            break;
                        case <= 255:
                            SetGrayscale(colourId - 232);
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }

                int GetNext()
                {
                    enumerator.MoveNext();
                    return enumerator.Current;
                }
            },

            [39] = (_, textSource) => {
                if (textSource._currentProperties.ForegroundBrush?.Opacity == textSource._defaultProperties.ForegroundBrush?.Opacity) {
                    textSource._currentProperties.ForegroundBrushOverwrite = null;
                    return;
                }

                if (textSource._defaultProperties.ForegroundBrush is not SolidColorBrush defaultSolidColorBrush)
                    return;

                textSource._currentProperties.ForegroundBrushOverwrite ??= new SolidColorBrush {
                    Opacity = textSource._currentProperties.ForegroundBrush?.Opacity ?? 1,
                };
                textSource._currentProperties.ForegroundBrushOverwrite.Color = defaultSolidColorBrush.Color;
            },

            #region bright colours
            // Set bright black
            [90] = ForegroundApplicatorFactory(Colors.DimGray),
            // Set bright red
            [91] = ForegroundApplicatorFactory(Colors.Red),
            // Set bright green
            [92] = ForegroundApplicatorFactory(Colors.LimeGreen),
            // Set bright yellow
            [93] = ForegroundApplicatorFactory(Colors.Yellow),
            // Set bright blue
            [94] = ForegroundApplicatorFactory(Colors.Blue),
            // Set bright magenta
            [95] = ForegroundApplicatorFactory(Colors.Magenta),
            // Set bright cyan
            [96] = ForegroundApplicatorFactory(Colors.Cyan),
            // Set bright white
            [97] = ForegroundApplicatorFactory(Colors.White),
            #endregion

            #endregion

            #region background colouring

            #region dark colours
            // Set black
            [40] = BackgroundApplicatorFactory(Colors.Black),
            // Set red
            [41] = BackgroundApplicatorFactory(Colors.DarkRed),
            // Set green
            [42] = BackgroundApplicatorFactory(Colors.DarkGreen),
            // Set yellow
            [43] = BackgroundApplicatorFactory(Colors.Olive),
            // Set blue
            [44] = BackgroundApplicatorFactory(Colors.DarkBlue),
            // Set magenta
            [45] = BackgroundApplicatorFactory(Colors.DarkMagenta),
            // Set cyan
            [46] = BackgroundApplicatorFactory(Colors.DarkCyan),
            // Set white
            [47] = BackgroundApplicatorFactory(Colors.DarkGray),
            #endregion

            [48] = (enumerator, textSource) => {
                switch (GetNext()) {
                    case 2:
                        // set truecolour
                        SetTrueColour();
                        break;
                    case 5:
                        // Set 256-colour
                        Set256Colour();
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }

                void SetRgb(byte r, byte g, byte b)
                {
                    BackgroundApplicatorFactory(Color.FromRgb(r, g, b))(enumerator, textSource);
                }

                void SetTrueColour()
                {
                    var (r, g, b) = (GetNext(), GetNext(), GetNext());
                    SetRgb((byte)r, (byte)g, (byte)b);
                }

                void Set3BitColourDepth(int index)
                {
                    var r = index / 36 * 51;
                    var g = index % 36 / 6 * 51;
                    var b = index % 6 * 51;
                    SetRgb((byte)r, (byte)g, (byte)b);
                }

                void SetGrayscale(int index)
                {
                    var component = (byte)(index * 10 + 8);
                    SetRgb(component, component, component);
                }

                void Set256Colour()
                {
                    var colourId = GetNext();
                    switch (colourId) {
                        case <= 7:
                            GraphicsModeApplicators[40 + colourId](enumerator, textSource);
                            break;
                        case <= 15:
                            GraphicsModeApplicators[92 + colourId](enumerator, textSource);
                            break;
                        case <= 231:
                            Set3BitColourDepth(colourId - 16);
                            break;
                        case <= 255:
                            SetGrayscale(colourId - 232);
                            break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }

                int GetNext()
                {
                    enumerator.MoveNext();
                    return enumerator.Current;
                }
            },

            [49] = (enumerator, textSource) => {
                // Reset
                if (textSource._currentProperties.BackgroundBrush?.Opacity == textSource._defaultProperties.BackgroundBrush?.Opacity) {
                    textSource._currentProperties.BackgroundBrushOverwrite = null;
                    return;
                }

                if (textSource._defaultProperties.BackgroundBrush is not SolidColorBrush defaultSolidColorBrush)
                    return;

                textSource._currentProperties.BackgroundBrushOverwrite ??= new SolidColorBrush {
                    Opacity = textSource._currentProperties.BackgroundBrush?.Opacity ?? 1,
                };
                textSource._currentProperties.BackgroundBrushOverwrite.Color = defaultSolidColorBrush.Color;
            },

            #region bright colours
            // Set bright black
            [100] = BackgroundApplicatorFactory(Colors.DimGray),
            // Set bright red
            [101] = BackgroundApplicatorFactory(Colors.Red),
            // Set bright green
            [102] = BackgroundApplicatorFactory(Colors.LimeGreen),
            // Set bright yellow
            [103] = BackgroundApplicatorFactory(Colors.Yellow),
            // Set bright blue
            [104] = BackgroundApplicatorFactory(Colors.Blue),
            // Set bright magenta
            [105] = BackgroundApplicatorFactory(Colors.Magenta),
            // Set bright cyan
            [106] = BackgroundApplicatorFactory(Colors.Cyan),
            // Set bright white
            [107] = BackgroundApplicatorFactory(Colors.White),
            #endregion

            #endregion
        };

        private static void ApplyGraphicsModes(IEnumerable<int> graphicsArguments, AnsiTextSource textSource)
        {
            using var graphicsArgumentEnumerator = graphicsArguments.GetEnumerator();

            while (graphicsArgumentEnumerator.MoveNext()) {
                if (!GraphicsModeApplicators.TryGetValue(graphicsArgumentEnumerator.Current, out var applicator)) continue;
                applicator(graphicsArgumentEnumerator, textSource);
            }
        }

        private readonly string _text;
        private readonly TextRunProperties _defaultProperties;

        private readonly StringBuilder _runTextBuilder;
        private StringBuilder? _escapeBuilder;

        private int _currentIndex;
        private ParseState _currentState;
        private readonly OverwriteTextRunProperties _currentProperties;

        public AnsiTextSource(string text, TextRunProperties defaultProperties)
        {
            _text = text;
            _defaultProperties = defaultProperties;
            _runTextBuilder = new();
            _currentProperties = new OverwriteTextRunProperties(_defaultProperties);
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
                        return new AnsiTextCharacters(_runTextBuilder.ToString(), _currentProperties.Freeze()) {
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
                        ApplyGraphicsModes(parameterValues, this);
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
                return new AnsiTextCharacters(_runTextBuilder.ToString(), _currentProperties.Freeze()) {
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

    public class AnsiEscapeSyntaxException : ArgumentException
    {
        public AnsiEscapeSyntaxException() { }

        public AnsiEscapeSyntaxException(string message) : base(message) { }

        public AnsiEscapeSyntaxException(string message, Exception inner) : base(message, inner) { }
    }
}
