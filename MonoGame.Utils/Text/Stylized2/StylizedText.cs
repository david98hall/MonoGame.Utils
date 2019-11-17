using MonoGame.Utils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MonoGame.Utils.Text.Stylized2
{
    /// <summary>
    /// A stylized text containing parts with different colors, fonts, opacities, et cetera.
    /// It's enumerable over tuples of text rows and their sizes.
    /// </summary>
    /// <typeparam name="C">A color type.</typeparam>
    /// <typeparam name="F">A font type.</typeparam>    
    public abstract class StylizedText<C, F> : IEnumerable<(IEnumerable<TextPart<C, F>> TextParts, (float Width, float Height) RowSize)>
    {

        #region Properties
        /// <summary>
        /// The alignment of this text.
        /// </summary>
        public TextAlignment Alignment { get; set; }

        /// <summary>
        /// The text color if nothing else is specified.
        /// </summary>
        public C DefaultColor
        {
            get => defaultColor;
            set
            {
                defaultColor = value;
                UpdateRows();
            }
        }
        private C defaultColor;

        /// <summary>
        /// The text font if nothing else is specified.
        /// </summary>
        public F DefaultFont
        {
            get => defaultFont;
            set
            {
                defaultFont = value;
                UpdateRows();
            }
        }
        private F defaultFont;

        /// <summary>
        /// The text opacity if nothing else is specified.
        /// </summary>
        public float DefaultOpacity
        {
            get => defaultOpacity;
            set
            {
                if (defaultOpacity < 0 || defaultOpacity > 1)
                {
                    throw new Exception("The opacity has to be in the range [0, 1]");
                }
                defaultOpacity = value;
                UpdateRows();
            }
        }
        private float defaultOpacity = 1;

        /// <summary>
        /// The raw text that is parsed.
        /// </summary>
        public string Text
        {
            get => text;
            set
            {
                text = value;
                UpdateRows();
            }
        }
        private string text;

        /// <summary>
        /// The size of this text.
        /// </summary>
        public (float Width, float Height) Size { get; private set; }

        /// <summary>
        /// The spacing between each row in this text.
        /// </summary>
        public float RowSpacing
        {
            get => rowSpacing;
            set
            {
                rowSpacing = value;
                Size = GetTextSize();
            }
        }
        private float rowSpacing;

        /// <summary>
        /// The character for defining new lines in the raw text.
        /// </summary>
        public char NewLine
        {
            get => newLineChar;
            set
            {
                newLineChar = value;
                UpdateRows();
            }
        }
        private char newLineChar = '\n';

        /// <summary>
        /// The character for separating styles in style blocks in the raw text.
        /// </summary>
        public char StyleSeparator
        {
            get => styleSeparator;
            set
            {
                styleSeparator = value;
                UpdateRows();
            }
        }
        private char styleSeparator = ',';

        /// <summary>
        /// The character representing equality for styles and their values in style blocks in the raw text.
        /// </summary>
        public char StyleEquality
        {
            get => styleEquality;
            set
            {
                styleEquality = value;
                UpdateRows();
            }
        }
        private char styleEquality = '=';

        /// <summary>
        /// The character sequences in the raw text that are equivalent to this class' default style
        /// </summary>
        public string[] DefaultStyleIdentifiers
        {
            get => defaultStyleIdentifier;
            set
            {
                defaultStyleIdentifier = value;
                UpdateRows();
            }
        }
        private string[] defaultStyleIdentifier = new string[] { "default", "d" };

        /// <summary>
        /// The max width of this text (new rows are added if a row's width exceeds this).
        /// </summary>
        public float MaxWidth
        {
            get => maxWidth;
            set
            {
                maxWidth = value;
                UpdateRows();
            }
        }
        private float maxWidth;
        #endregion

        // Stylized rows in the text
        protected IEnumerable<(IEnumerable<TextPart<C, F>> TextParts, (float Width, float Height) RowSize)> rows;

        // The Regex pattern for identifying style blocks in the raw text
        private static readonly string styleBlockPattern = @"\[[a-zA-Z0-9=,.\s]+\]";

        /// <summary>
        /// Sets values and parses the passed raw text.
        /// </summary>
        /// <param name="text">The raw text.</param>
        /// <param name="defaultColor">The text color if nothing else is specified.</param>
        /// <param name="defaultFont">The text font if nothing else is specified.</param>
        /// <param name="maxWidth">The max width of this text (new rows are added if a row's width exceeds this).</param>
        /// <param name="rowSpacing">The spacing between rows in this text.</param>
        public StylizedText(string text, C defaultColor, F defaultFont, float maxWidth = float.MaxValue, float rowSpacing = 3)
        {
            Alignment = TextAlignment.LEFT;
            this.text = text;
            this.defaultColor = defaultColor;
            this.defaultFont = defaultFont;
            this.maxWidth = maxWidth;
            this.rowSpacing = rowSpacing;
            UpdateRows();
        }

        #region Abstract
        /// <summary>
        /// Gets the color based on its name and opacity (alpha).
        /// </summary>
        /// <param name="colorName">The name of the color to get.</param>
        /// <param name="alpha">The opacity of the color.</param>
        /// <returns>The color with the specified name and opacity.</returns>
        protected abstract C GetColor(string colorName, float alpha = 1);

        /// <summary>
        /// Gets the same color that's passed but with a specific opacity.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="alpha">The opacity of the color.</param>
        /// <returns>The color with the specified opacity.</returns>
        protected abstract C GetColor(C color, float alpha);

        /// <summary>
        /// Gets the font based on its name.
        /// </summary>
        /// <param name="fontName">The name of the font to get.</param>
        /// <returns>The font with the specified name.</returns>
        protected abstract F GetFont(string fontName);

        /// <summary>
        /// Measures a string based on a given font.
        /// </summary>
        /// <param name="font">The font to measure with.</param>
        /// <param name="text">The text to measure.</param>
        /// <returns>The bounds of the text.</returns>
        protected abstract (float Width, float Height) MeasureString(F font, string text);
        #endregion

        #region Parsing
        // Parses rows and updates the text size
        private void UpdateRows()
        {
            rows = ParseRows();
            Size = GetTextSize();
        }

        // Parses rows after parsing the text as a whole
        private IEnumerable<(IEnumerable<TextPart<C, F>> TextParts, (float Width, float Height) RowSize)> ParseRows()
        {
            var stylizedRows = new LinkedList<LinkedList<TextPart<C, F>>>();

            // Parse the whole text and traverse all of its stylized parts
            foreach (var textPart in ParseText())
            {
                // Get all rows within the text part
                var textPartRows = textPart.Text
                    .Split(NewLine)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0);

                // Traverse all rows within the text part
                foreach (var textPartRow in textPartRows)
                {
                    // Fit each text part row to the max width
                    foreach (var fittedRow in FitTextPart(textPart, MaxWidth))
                    {
                        // Create new row and add the fitted text part to it
                        stylizedRows.AddLast(new LinkedList<TextPart<C, F>>());
                        var currentRow = stylizedRows.Last.Value;
                        currentRow.AddLast(fittedRow);
                    }
                }
            }

            // Return the rows and their respective sizes in a tuple
            return stylizedRows.Select(r => (r as IEnumerable<TextPart<C, F>>, GetRowSize(r)));
        }

        private IEnumerable<TextPart<C, F>> FitTextPart(TextPart<C, F> textPart, float maxWidth)
        {
            if (!IsWider(textPart, maxWidth))
            {
                // Not wider than the max width, return the text part "as is"
                return new TextPart<C, F>[] { GetTrimmedTextPart(textPart) };
            }

            var textPartRows = new LinkedList<TextPart<C, F>>();

            // Traverse all characters in the text part
            var textPartRow = "";
            foreach (var c in textPart.Text.ToCharArray())
            {
                var tmpPartRow = textPartRow + c;
                if (IsWider(tmpPartRow, textPart.Font, maxWidth))
                {
                    // If the next string would be wider than the max width, add the current one as a row
                    textPartRows.AddLast(GetTrimmedTextPart(textPartRow, textPart.Color, textPart.Font));

                    // Reset the row string to the character that didn't fit on the row
                    textPartRow = "" + c;
                }
                else
                {
                    // The string is not wider than the max width, update the row text
                    textPartRow = tmpPartRow;
                }
            }

            // Add the remaining text to a row of its own
            textPartRows.AddLast(GetTrimmedTextPart(textPartRow, textPart.Color, textPart.Font));

            // Return all rows after being fitted to the max width
            return textPartRows;
        }

        private IEnumerable<TextPart<C, F>> ParseText()
        {
            var text = Text.Trim().Replace("\r\n", "" + NewLine).Replace('\r', NewLine);
            var stylizedText = new LinkedList<TextPart<C, F>>();
            var styleBlockCollection = Regex.Matches(text, styleBlockPattern);

            if (styleBlockCollection.Count == 0)
            {
                // No specified styles, return the default
                stylizedText.AddFirst(new TextPart<C, F>(text, DefaultColor, DefaultFont));
                return stylizedText;
            }

            var styleBlocks = styleBlockCollection.Cast<Match>().Select(m => m.Value).ToArray();
            var stringParts = Regex.Split(text, styleBlockPattern).Where(s => s.Trim().Length > 0).ToArray();

            var allParts = stringParts.Interleave(styleBlocks);
            var partStack = new Stack<string>(allParts);

            string parentColorName = null;
            F parentFont = DefaultFont;
            float parentOpacity = DefaultOpacity;

            while (partStack.Count > 0)
            {
                var pop = partStack.Pop();

                if (Regex.IsMatch(pop, styleBlockPattern))
                {
                    // Update the parent styles
                    var (ColorName, Font, Opacity) = ExtractStyles(pop, parentColorName, parentFont);
                    parentColorName = ColorName;
                    parentFont = Font;
                    parentOpacity = Opacity;

                    // The last style block in a sequence should count (overrides the others)
                    var nextPop = partStack.Pop();
                    while (Regex.IsMatch(nextPop, styleBlockPattern))
                    {
                        nextPop = partStack.Pop();
                    }

                    // Push the string part back
                    partStack.Push(nextPop);
                }
                else
                {
                    // Get the color of the text part
                    C color;
                    if (parentColorName == null)
                    {
                        color = GetColor(DefaultColor, parentOpacity);
                    }
                    else
                    {
                        color = GetColor(parentColorName, parentOpacity);
                    }

                    // Stylize and add the current text part (not a style block)
                    stylizedText.AddFirst(new TextPart<C, F>(pop, color, parentFont));
                }
            }

            return stylizedText;
        }

        #region Size calculations
        internal (float Width, float Height) GetTextSize()
        {
            float textWidth = 0;
            float textHeight = 0;

            foreach (var (_, RowSize) in this)
            {
                if (RowSize.Width < 0 || RowSize.Height < 0)
                {
                    // If a row is not measured, the text's width can't be
                    return (-1, -1);
                }

                // Width
                if (RowSize.Width > textWidth)
                {
                    textWidth = RowSize.Width;
                }

                // Height
                textHeight += RowSize.Height + rowSpacing;
            }

            return (textWidth, textHeight - rowSpacing);
        }

        public (float Width, float Height) GetRowSize(IEnumerable<TextPart<C, F>> rowText)
        {
            float rowWidth = 0;
            float rowHeight = 0;
            foreach (var word in rowText)
            {
                if (word.Font == null)
                {
                    // If a font is not available, the row can't be measured.
                    throw new NullReferenceException("A font is null!");
                }

                // Row Width
                var (Width, Height) = MeasureString(word.Font, word.Text);
                rowWidth += Width;

                // Row Height
                if (Height > rowHeight)
                {
                    rowHeight = Height;
                }
            }

            return (rowWidth, rowHeight);
        }

        private bool IsWider(TextPart<C, F> textPart, float maxWidth)
        {
            return IsWider(textPart.Text, textPart.Font, maxWidth);
        }

        private bool IsWider(string text, F font, float maxWidth)
        {
            return MeasureString(font, text).Width > maxWidth;
        }
        #endregion

        #region Trimming
        private TextPart<C, F> GetTrimmedTextPart(TextPart<C, F> textPart)
        {
            return GetTrimmedTextPart(textPart.Text, textPart.Color, textPart.Font);
        }

        private TextPart<C, F> GetTrimmedTextPart(string text, C color, F font)
        {
            return new TextPart<C, F>(text.Trim(), color, font);
        }
        #endregion

        // Extracts styles from a code block
        private (string ColorName, F Font, float Opacity) ExtractStyles(string styleBlock, string parentColorName, F parentFont)
        {
            // Parent style
            string colorName = parentColorName;
            F font = parentFont;
            float opacity = DefaultOpacity;                                 // TODO This is not necessarily the parent's opacity

            // Remove the style block's brackets
            styleBlock = styleBlock.Substring(1, styleBlock.Length - 2);

            if (defaultStyleIdentifier.Contains(styleBlock))
            {
                // If the style block's content is equal to a default style identifier,
                // return the default style
                return (null, DefaultFont, DefaultOpacity);
            }

            // Traverse all styles in the block
            var styleStrings = Regex.Replace(styleBlock, @"\s", "").Split(StyleSeparator);
            foreach (var s in styleStrings)
            {
                try
                {
                    // Extract the style's name and value
                    var styleName = s.Substring(0, s.IndexOf(StyleEquality));
                    var styleValue = s.Substring(styleName.Length + 1, s.Length - styleName.Length - 1);

                    // Try to find out which style type it is and get the corresponding value
                    if (Enum.TryParse(styleName.ToUpper(), out Style style))
                    {
                        switch (style)
                        {
                            case Style.COLOR:
                                colorName = styleValue;
                                break;
                            case Style.FONT:
                                font = GetFont(styleValue);
                                break;
                            case Style.OPACITY:
                                opacity = float.Parse(styleValue);
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    // throw new Exception("Exception: Unidentifiable style!");
                }
            }

            return (colorName, font, opacity);
        }

        #region Internal Datatypes
        /// <summary>
        /// Supported styles in this parser.
        /// </summary>
        internal enum Style
        {
            COLOR, FONT, OPACITY
        }

        /// <summary>
        /// Helper class, makes the code cleaner.
        /// </summary>
        internal class Color
        {

            internal C Value { get; private set; }

            internal string Name { get; private set; }

            internal Color(C color, string name)
            {
                Value = color;
                Name = name;
            }
        }
        #endregion

        #endregion

        #region IEnumerable
        public IEnumerator<(IEnumerable<TextPart<C, F>> TextParts, (float Width, float Height) RowSize)> GetEnumerator()
        {
            return rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

    }

    /// <summary>
    /// Text alignment types.
    /// </summary>
    public enum TextAlignment
    {
        LEFT, CENTER, RIGHT
    }

    /// <summary>
    /// A text part consisting of a string, a color and a font.
    /// </summary>
    /// <typeparam name="C">A color type.</typeparam>
    /// <typeparam name="F">A font type.</typeparam>
    public class TextPart<C, F>
    {
        /// <summary>
        /// The text value.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The color of this text.
        /// </summary>
        public C Color { get; set; }

        /// <summary>
        /// The font of this text.
        /// </summary>
        public F Font { get; set; }

        /// <summary>
        /// Sets properties.
        /// </summary>
        /// <param name="text">The text value.</param>
        /// <param name="color">The color of this text.</param>
        /// <param name="font">The font of this text.</param>
        public TextPart(string text, C color, F font)
        {
            Text = text;
            Font = font;
            Color = color;
        }
    }

}
