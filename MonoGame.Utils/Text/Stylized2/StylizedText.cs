using MonoGame.Utils.Tuples;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MonoGame.Utils.Text.Stylized2
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="C">A color type.</typeparam>
    /// <typeparam name="F">A font type.</typeparam>    
    public abstract class StylizedText<C, F> : IEnumerable<(IEnumerable<TextPart<C, F>> TextParts, (float Width, float Height) RowSize)>
    {

        public enum TextAlignment
        {
            LEFT, CENTER, RIGHT
        }

        public TextAlignment Alignment { get; set; }

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


        protected IEnumerable<(IEnumerable<TextPart<C, F>> TextParts, (float Width, float Height) RowSize)> rows;

        private static readonly string styleBlockPattern = @"\[[a-zA-Z0-9=,.\s]+\]";

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

        public (float Width, float Height) Size { get; private set; }

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

        public StylizedText(string text, C defaultColor, F defaultFont)
        {
            Alignment = TextAlignment.LEFT;
            this.text = text;
            this.defaultColor = defaultColor;
            this.defaultFont = defaultFont;
            UpdateRows();
        }

        protected abstract C GetColor(string colorName, float alpha = 1);

        protected abstract C GetColor(C color, float alpha);

        protected abstract F GetFont(string fontName);

        protected abstract (float Width, float Height) MeasureString(F font, string text);

        private void UpdateRows()
        {
            rows = ParseRows();
            Size = GetTextSize();
        }

        private IEnumerable<(IEnumerable<TextPart<C, F>> TextParts, (float Width, float Height) RowSize)> ParseRows(float maxWidth = float.MaxValue)
        {
            var stylizedRows = new LinkedList<LinkedList<TextPart<C, F>>>();

            foreach (var textPart in ParseText())
            {
                var textPartRows = textPart.Text
                    .Split(NewLine)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0);

                foreach (var textPartRow in textPartRows)
                {
                    // Create new row
                    stylizedRows.AddLast(new LinkedList<TextPart<C, F>>());
                    var currentRow = stylizedRows.Last.Value;

                    currentRow.AddLast(new TextPart<C, F>(textPartRow, textPart.Color, textPart.Font));
                }
            }

            return stylizedRows.Select(r => (r as IEnumerable<TextPart<C, F>>, GetRowSize(r)));
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

            var allParts = Interleave(stringParts, styleBlocks);
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
                var wordSize = MeasureString(word.Font, word.Text);
                rowWidth += wordSize.Width;

                // Row Height
                if (wordSize.Height > rowHeight)
                {
                    rowHeight = wordSize.Height;
                }
            }

            return (rowWidth, rowHeight);
        }

        private T[] Interleave<T>(T[] array1, T[] array2)
        {
            var result = new T[array1.Length + array2.Length];

            var array1Index = 0;
            var array2Index = 0;
            for (int i = 0; i < result.Length; i++)
            {
                if (i % 2 == 0)
                {
                    if (array1Index < array1.Length)
                    {
                        result[i] = array1[array1Index];
                        array1Index++;
                    }
                }
                else if (array2Index < array2.Length)
                {
                    result[i] = array2[array2Index];
                    array2Index++;
                }
            }

            return result;
        }

        private (string ColorName, F Font, float Opacity) ExtractStyles(string styleBlock, string parentColorName, F parentFont)
        {
            string colorName = parentColorName;
            F font = parentFont;
            float opacity = 1;

            styleBlock = styleBlock.Substring(1, styleBlock.Length - 2);

            if (defaultStyleIdentifier.Contains(styleBlock))
            {
                return (null, DefaultFont, DefaultOpacity);
            }

            var styleStrings = Regex.Replace(styleBlock, @"\s", "").Split(StyleSeparator);
            foreach (var s in styleStrings)
            {
                try
                {
                    var styleName = s.Substring(0, s.IndexOf(StyleEquality));
                    var styleValue = s.Substring(styleName.Length + 1, s.Length - styleName.Length - 1);

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

        public IEnumerator<(IEnumerable<TextPart<C, F>> TextParts, (float Width, float Height) RowSize)> GetEnumerator()
        {
            return rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal enum Style
        {
            COLOR, FONT, OPACITY
        }

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

    }

    public class TextPart<C, F>
    {
        public string Text { get; set; }

        public C Color { get; set; }

        public F Font { get; set; }

        public TextPart(string text, C color, F font)
        {
            Text = text;
            Font = font;
            Color = color;
        }
    }

}
