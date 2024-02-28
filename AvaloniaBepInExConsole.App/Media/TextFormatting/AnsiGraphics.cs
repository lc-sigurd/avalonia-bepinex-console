using System;
using System.Collections.Generic;
using Avalonia.Media;

namespace Sigurd.AvaloniaBepInExConsole.App.Media.TextFormatting;

public static class AnsiGraphics
{
    public delegate void GraphicsModeApplicator(IEnumerator<int> graphicsArgumentEnumerator, OverwriteTextRunProperties properties);

    public static void ApplyGraphicsModes(IEnumerable<int> graphicsArguments, OverwriteTextRunProperties properties)
    {
        using var graphicsArgumentEnumerator = graphicsArguments.GetEnumerator();

        while (graphicsArgumentEnumerator.MoveNext()) {
            if (!ModeApplicators.TryGetValue(graphicsArgumentEnumerator.Current, out var applicator)) continue;
            applicator(graphicsArgumentEnumerator, properties);
        }
    }

    private static GraphicsModeApplicator ForegroundApplicatorFactory(Color colorToApply)
    {
        return (_, properties) => {
            properties.ForegroundBrushOverwrite ??= new SolidColorBrush {
                Opacity = properties.ForegroundBrush?.Opacity ?? 1,
            };
            properties.ForegroundBrushOverwrite.Color = colorToApply;
        };
    }

    private static GraphicsModeApplicator BackgroundApplicatorFactory(Color colorToApply)
    {
        return (_, properties) => {
            properties.BackgroundBrushOverwrite ??= new SolidColorBrush {
                Opacity = properties.BackgroundBrush?.Opacity ?? 1,
            };
            properties.BackgroundBrushOverwrite.Color = colorToApply;
        };
    }

    public static Dictionary<int, GraphicsModeApplicator> ModeApplicators { get; } = new() {
        #region basic
        [0] = (enumerator, properties) => {
            // reset all
            properties.ResetOverwrites();
        },
        [1] = (enumerator, properties) => {
            // set bold weight
        },
        [2] = (enumerator, properties) => {
            // set faint weight
        },
        [22] = (enumerator, properties) => {
            // reset weight (set regular weight)
        },
        [3]= (enumerator, properties) => {
            // set italic
        },
        [23] = (enumerator, properties) => {
            // reset italic
        },
        [4] = (enumerator, properties) => {
            // set underline
        },
        [24] = (enumerator, properties) => {
            // reset underline
        },
        [5] = (enumerator, properties) => {
            // set 'blinking mode'
        },
        [25] = (enumerator, properties) => {
            // reset blinding mode
        },
        [7] = (enumerator, properties) => {
            // set inverse/reverse mode
        },
        [27] = (enumerator, properties) => {
            // reset inverse/reverse mode
        },
        [8] = (enumerator, properties) => {
            // set hidden/invisible mode
        },
        [28] = (enumerator, properties) => {
            // reset hidden/invisible mode
        },
        [9] = (enumerator, properties) => {
            // set strikethrough
        },
        [29] = (enumerator, properties) => {
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

        [38] = (enumerator, properties) => {
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
                ForegroundApplicatorFactory(Color.FromRgb(r, g, b))(enumerator, properties);
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
                        ModeApplicators[30 + colourId](enumerator, properties);
                        break;
                    case <= 15:
                        ModeApplicators[82 + colourId](enumerator, properties);
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

        [39] = (_, properties) => {
            if (properties.ForegroundBrush?.Opacity == properties.Defaults.ForegroundBrush?.Opacity) {
                properties.ForegroundBrushOverwrite = null;
                return;
            }

            if (properties.Defaults.ForegroundBrush is not SolidColorBrush defaultSolidColorBrush)
                return;

            properties.ForegroundBrushOverwrite ??= new SolidColorBrush {
                Opacity = properties.ForegroundBrush?.Opacity ?? 1,
            };
            properties.ForegroundBrushOverwrite.Color = defaultSolidColorBrush.Color;
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

        [48] = (enumerator, properties) => {
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
                BackgroundApplicatorFactory(Color.FromRgb(r, g, b))(enumerator, properties);
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
                        ModeApplicators[40 + colourId](enumerator, properties);
                        break;
                    case <= 15:
                        ModeApplicators[92 + colourId](enumerator, properties);
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

        [49] = (enumerator, properties) => {
            // Reset
            if (properties.BackgroundBrush?.Opacity == properties.Defaults.BackgroundBrush?.Opacity) {
                properties.BackgroundBrushOverwrite = null;
                return;
            }

            if (properties.Defaults.BackgroundBrush is not SolidColorBrush defaultSolidColorBrush)
                return;

            properties.BackgroundBrushOverwrite ??= new SolidColorBrush {
                Opacity = properties.BackgroundBrush?.Opacity ?? 1,
            };
            properties.BackgroundBrushOverwrite.Color = defaultSolidColorBrush.Color;
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
}
