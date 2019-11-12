using MonoGame.Utils.Tuples;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.Utils.Text
{
    public partial class StylizedTextParser
    {
        /// <summary>
        /// Parses the text.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>The parsed text.</returns>
        public (IEnumerable<(IEnumerable<Word> RowText, MutableTuple<float, float> RowSize)> Rows,
            (float Width, float Height) TextSize)
            ParseText(string text)
        {
            var stylizedText = ParseTextRows(text.Trim());
            return (stylizedText, GetTextSize(stylizedText, RowSpacing));
        }

        /// <summary>
        /// Parses the text and fits it to a max width, remaking the rows if necessary
        /// </summary>
        /// <param name="text">The text to parse and fit.</param>
        /// <param name="maxWidth">The max width of the text.</param>
        /// <returns>The parsed and fitted text.</returns>
        public (IEnumerable<(IEnumerable<Word> RowText, MutableTuple<float, float> RowSize)> Rows,
            (float Width, float Height) TextSize)
            ParseAndFitTextHorizontally(string text, float maxWidth)
        {
            var (StylizedText, TextSize) = ParseText(text);

            // If the text already fits, return it as is
            if (TextSize.Width <= maxWidth)
            {
                return (StylizedText, TextSize);
            }

            // Otherwise, redo the rows
            var stylizedTextList = StylizedText as List<(IEnumerable<Word> RowText, MutableTuple<float, float> RowSize)>;
            var newStylizedText = FitTextHorizontally(stylizedTextList, maxWidth);
            return (newStylizedText, GetTextSize(newStylizedText, RowSpacing));
        }

        public IEnumerable<(IEnumerable<Word> RowText, MutableTuple<float, float> RowSize)>
            ParseTextRows(string text)
        {
            var rowCount = text.Count(t => t == NewLine) + 1;
            text = text.Replace("\r\n", "" + NewLine).Replace('\r', NewLine);

            // Add empty lists and default tuples
            var stylizedText = new List<(IEnumerable<Word>, MutableTuple<float, float>)>(rowCount);
            for (int i = 0; i < rowCount; i++)
            {
                stylizedText.Add((
                    new LinkedList<Word>(),
                    new MutableTuple<float, float>(-1, -1)
                ));
            }

            // Build rows
            var stylizedWords = ParseWords(text);
            var rowIndex = 0;
            foreach (var word in stylizedWords)
            {
                // Split the word into parts where there is a new line
                string[] wordParts = word.Text.Split(NewLine);
                foreach (var wordPart in wordParts)
                {
                    // Add the word part on the correct row
                    var textRow = stylizedText[rowIndex].Item1 as LinkedList<Word>;
                    textRow.AddLast(new Word(wordPart, word.Font, word.Color));

                    // Go to the next row
                    rowIndex++;
                }

                // The last part of the word after splitting does 
                // not end with a new line, decrement to fix this
                rowIndex--;
            }

            foreach (var row in stylizedText)
            {
                // Calculate each row size
                var rowSize = GetRowSize(row.Item1);
                row.Item2.Item1 = rowSize.Item1;
                row.Item2.Item2 = rowSize.Item2;

                // Remove any escape hatches
                foreach (var word in row.Item1)
                {
                    word.Text = word.Text.Replace(EscapeCharacter + "{", "{");
                    word.Text = word.Text.Replace(EscapeCharacter + "}", "}");
                    word.Text = word.Text.Replace(EscapeCharacter + "[", "[");
                    word.Text = word.Text.Replace(EscapeCharacter + "]", "]");
                }

            }

            return stylizedText;
        }

        public IEnumerable<Word> ParseWords(string text)
        {
            return ParseWords(new Word(text, DefaultFont, DefaultColor));
        }

        public static (float Width, float Height) GetTextSize(
            IEnumerable<(IEnumerable<Word> RowText,
            MutableTuple<float, float> RowSize)> text,
            float rowSpacing)
        {
            float textWidth = 0;
            float textHeight = 0;

            foreach (var (_, RowSize) in text)
            {
                if (RowSize.Item1 < 0 || RowSize.Item2 < 0)
                {
                    // If a row is not measured, the text's width can't be
                    return (-1, -1);
                }

                // Width
                if (RowSize.Item1 > textWidth)
                {
                    textWidth = RowSize.Item1;
                }

                // Height
                textHeight += RowSize.Item2 + rowSpacing;
            }

            return (textWidth, textHeight - rowSpacing);
        }

        public static MutableTuple<float, float> GetRowSize(IEnumerable<Word> rowText)
        {
            float rowWidth = 0;
            float rowHeight = 0;
            foreach (var word in rowText)
            {
                if (word.Font == null)
                {
                    // If a font is not available, the row can't be measured.
                    var message = "StylizedTextParser: A font is null and the row" +
                        " size can therefore not be calculated! " +
                        "Check that the DefaultFont property is not null!";
                    throw new NullReferenceException(message);
                }

                // Row Width
                var wordSize = word.Font.MeasureString(word.Text);
                rowWidth += wordSize.X;

                // Row Height
                if (wordSize.Y > rowHeight)
                {
                    rowHeight = wordSize.Y;
                }
            }

            return new MutableTuple<float, float>(rowWidth, rowHeight);
        }

    }

}
