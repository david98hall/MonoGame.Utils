using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Utils.Tuples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MonoGame.Utils.Text
{
    public partial class StylizedTextParser
    {

        // Fits the text to a max width and remakes the rows if necessary
        private IEnumerable<(IEnumerable<TextPart<Color, SpriteFont>> RowText, MutableTuple<float, float> RowSize)>
            FitTextHorizontally(
                List<(IEnumerable<TextPart<Color, SpriteFont>> RowText, MutableTuple<float, float> RowSize)> rows,
                float maxWidth)
        {
            var newRows = new LinkedList<(IEnumerable<TextPart<Color, SpriteFont>> RowText, MutableTuple<float, float>)>();

            if (maxWidth <= 0)
            {
                // No text can fit within a max width of zero or smaller; return nothing
                return newRows;
            }

            newRows.AddFirst((new LinkedList<TextPart<Color, SpriteFont>>(), new MutableTuple<float, float>(0, 0)));

            FitTextHorizontally(rows, 0, newRows, maxWidth, null);

            var emptyRows = new HashSet<(IEnumerable<TextPart<Color, SpriteFont>> RowText, MutableTuple<float, float> RowSize)>();
            foreach (var row in newRows)
            {
                var (RowText, RowSize) = row;

                TrimRow(RowText as LinkedList<TextPart<Color, SpriteFont>>);

                // Calculate row sizes
                var rowSize = GetRowSize(RowText);
                RowSize.Item1 = rowSize.Item1;
                RowSize.Item2 = rowSize.Item2;

                // Find empty rows
                var noWidth = RowSize.Item1 <= 0;
                if (noWidth)
                {
                    emptyRows.Add(row);
                }
            }

            // Remove empty rows
            foreach (var row in emptyRows)
            {
                newRows.Remove(row);
            }

            return newRows;
        }

        private void FitTextHorizontally(
            List<(IEnumerable<TextPart<Color, SpriteFont>> RowText, MutableTuple<float, float> RowSize)> rows,
            int currentRowIndex,
            LinkedList<(IEnumerable<TextPart<Color, SpriteFont>> RowText, MutableTuple<float, float> RowSize)> newRows,
            float maxWidth,
            IList<TextPart<Color, SpriteFont>> remainingWords)
        {
            var noMoreOriginalRows = currentRowIndex > rows.Count - 1;
            var currentRowCopy = noMoreOriginalRows
                ? new LinkedList<TextPart<Color, SpriteFont>>()
                : new LinkedList<TextPart<Color, SpriteFont>>(rows[currentRowIndex].RowText);

            var noRemainingWords = remainingWords == null || remainingWords.Count() == 0;
            if (noRemainingWords)
            {
                if (noMoreOriginalRows)
                {
                    // No more word parts, we are done! Break the recursion
                    return;
                }
            }
            else
            {
                // Add remaining words from the previous row
                for (int i = remainingWords.Count - 1; i >= 0; i--)
                {
                    currentRowCopy.AddFirst(remainingWords[i]);
                }
            }

            // Add words on the current row to the new row            
            var lastNewRowText = newRows.Last.Value.RowText as LinkedList<TextPart<Color, SpriteFont>>;
            var newRemainingWords = new List<TextPart<Color, SpriteFont>>();
            float newRowWidth = 0;
            int wordIndex = 0;
            foreach (var word in currentRowCopy)
            {
                if (word != null)
                {
                    var wordWidth = word.Font.MeasureString(word.Text).X;
                    var tmpRowWidth = newRowWidth + wordWidth;

                    if (tmpRowWidth > maxWidth)
                    {
                        // Split at whitespaces and add the parts until no more fit
                        var spaceWidth = word.Font.MeasureString(" ").X;
                        var wordParts = word.Text.Split(null);
                        int j;
                        for (j = 0; j < wordParts.Length; j++)
                        {
                            var wordPart = wordParts[j];
                            var wordPartWidth = word.Font.MeasureString(wordPart).X;
                            var tmpRowWidth1 = newRowWidth + wordPartWidth;
                            if (tmpRowWidth1 <= maxWidth)
                            {
                                // Add the word part since it fits on the current row
                                lastNewRowText.AddLast(new TextPart<Color, SpriteFont>(wordPart + " ", word.Color, word.Font));
                                newRowWidth = tmpRowWidth1 + spaceWidth;
                            }
                            else
                            {
                                // No more word parts fit on the current row
                                break;
                            }
                        }

                        if (lastNewRowText.Last == null)
                        {
                            // No word parts were added (none of them fit within the max width)
                            // Skip them and the rest of the text since 
                            // they will never fit if only split at whitespaces
                            return;
                        }

                        // The whitespace at the end of the last word of the row should not be there
                        lastNewRowText.Last.Value.Text = lastNewRowText.Last.Value.Text.TrimEnd();
                        newRowWidth -= spaceWidth;

                        // Add the remaining parts into one word
                        var remainingWordParts = "";
                        while (j < wordParts.Length)
                        {
                            remainingWordParts += wordParts[j] + " ";
                            j++;
                        }

                        // Union all left over word parts in one new TextPart<Color, SpriteFont>
                        var isRemainingParts = remainingWordParts.Length != 0;
                        var nextWord = isRemainingParts
                            ? new TextPart<Color, SpriteFont>(remainingWordParts, word.Color, word.Font)
                            : null;

                        // Get remaining words on this row
                        if (nextWord != null)
                        {
                            newRemainingWords.Add(nextWord);
                        }
                        var nextWordIndex = wordIndex + 1;
                        if (nextWordIndex < currentRowCopy.Count)
                        {
                            var numRemainingWords = currentRowCopy.Count - nextWordIndex;
                            newRemainingWords.AddRange(currentRowCopy.ToList().GetRange(nextWordIndex, numRemainingWords));
                        }

                        break;
                    }
                    else
                    {
                        // Adding the word does not exceed the max width
                        newRowWidth = tmpRowWidth;
                        lastNewRowText.AddLast(word);
                    }
                }

                wordIndex++;
            }

            // Go to the next original row
            newRows.AddLast((new LinkedList<TextPart<Color, SpriteFont>>(), new MutableTuple<float, float>(0, 0)));
            FitTextHorizontally(
                rows,
                currentRowIndex + 1,
                newRows,
                maxWidth,
                newRemainingWords);

        }

        private void TrimRow(LinkedList<TextPart<Color, SpriteFont>> row)
        {
            TrimRow(row, true);
            TrimRow(row, false);
        }

        private void TrimRow(LinkedList<TextPart<Color, SpriteFont>> row, bool fromLeft)
        {
            if (row.Count > 0)
            {
                var endWord = new TextPart<Color, SpriteFont>("", DefaultColor, DefaultFont);
                TextPart<Color, SpriteFont> currentWord;
                while (row.Count > 0)
                {
                    // Remove word from row
                    if (fromLeft)
                    {
                        currentWord = row.First.Value;
                        row.RemoveFirst();
                    }
                    else
                    {
                        currentWord = row.Last.Value;
                        row.RemoveLast();
                    }

                    var wordText = currentWord.Text;
                    bool isWhitespace = wordText.Trim().Length == 0;

                    // Trim ends
                    if (!isWhitespace)
                    {
                        if (fromLeft)
                        {
                            wordText = wordText.TrimStart();
                        }
                        else
                        {
                            wordText = wordText.TrimEnd();
                        }

                        endWord = new TextPart<Color, SpriteFont>(wordText, currentWord.Color, currentWord.Font);

                        break;
                    }
                }

                // Add the trimmed word back
                if (fromLeft)
                {
                    row.AddFirst(endWord);
                }
                else
                {
                    row.AddLast(endWord);
                }

            }
        }

        private IEnumerable<TextPart<Color, SpriteFont>> ParseWords(TextPart<Color, SpriteFont> word)
        {
            var stylizedWords = new List<TextPart<Color, SpriteFont>>();

            var startBraceCount = word.Text.Count(t => t == '{') - word.Text.Count(EscapeCharacter + "{");
            var endBraceCount = word.Text.Count(t => t == '}') - word.Text.Count(EscapeCharacter + "}");
            if (startBraceCount == 0 || endBraceCount == 0)
            {
                stylizedWords.AddRange(ParseStyle(word.Text, word.Font, word.Color));
                return stylizedWords;
            }

            var latestStartBrace = -1;

            char previousChar = '§';
            for (int i = 0; i < word.Text.Length; i++)
            {
                char c = word.Text[i];

                // Check for braced area without escape characters
                var previousWasEscapeChar = i > 0 && previousChar == EscapeCharacter;
                var firstCharOrNotEscapeChar = i == 0 || !previousWasEscapeChar;
                var startOfBracedArea = c == '{' && firstCharOrNotEscapeChar;
                var endOfBracedArea = c == '}' && latestStartBrace > -1 && firstCharOrNotEscapeChar;

                if (startOfBracedArea)
                {
                    latestStartBrace = i;
                }
                else if (endOfBracedArea)
                {
                    // Parse the words to the right of the braced area
                    var rightPart = ParseWords(new TextPart<Color, SpriteFont>(word.Text.Substring(i + 1), word.Color, word.Font));

                    // Extract the style of the first word to the right of the braced area 
                    // since the left side will have it as its default style
                    // var (_, FirstRightFont, FirstRightColor) = rightPart.First();
                    var rightPartWord = rightPart.First();

                    // Parse the words to the left of the braced area with the extracted style as the default
                    var leftWord = new TextPart<Color, SpriteFont>(word.Text.Substring(0, latestStartBrace),
                        rightPartWord.Color,
                        rightPartWord.Font);
                    var leftPart = ParseWords(leftWord);

                    // Add the words to the left to the result
                    stylizedWords.AddRange(leftPart);

                    // Parse the braced text and add the words to the result
                    var bracedText = word.Text.Substring(latestStartBrace + 1, i - latestStartBrace - 1);
                    foreach (var stylizedWord in ParseStyle(bracedText, word.Font, word.Color))
                    {
                        stylizedWords.AddRange(ParseWords(stylizedWord));
                    }

                    // Add the words to the right to the result
                    stylizedWords.AddRange(rightPart);

                    break;
                }

                previousChar = c;
            }

            return stylizedWords;
        }

        private (int LastStartBracket, int LastEndBracket) FindLastStyleBlock(string text)
        {
            int lastStartBracket = -1;
            int lastEndBracket = -1;
            var previousChar = '§';
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                // Check for block without escape characters
                var previousWasEscapeChar = i > 0 && previousChar == EscapeCharacter;
                var firstCharOrNotEscapeChar = i == 0 || !previousWasEscapeChar;
                var startOfBlock = c == '[' && firstCharOrNotEscapeChar;
                var endOfBlock = c == ']' && lastStartBracket != -1 && firstCharOrNotEscapeChar;
                if (startOfBlock)
                {
                    lastStartBracket = i;
                }
                else if (endOfBlock)
                {
                    lastEndBracket = i;
                }

                previousChar = c;
            }

            var nothingFound = lastStartBracket < 0 || lastEndBracket < 0;
            var wrongBracketOrder = lastStartBracket < 0 || lastEndBracket < 0 || lastStartBracket > lastEndBracket;
            if (nothingFound || wrongBracketOrder)
            {
                return (-1, -1);
            }

            return (lastStartBracket, lastEndBracket);
        }

        private IEnumerable<TextPart<Color, SpriteFont>> ParseStyle(string text, SpriteFont parentFont, Color parentColor)
        {
            // Try to look for the last style block
            var (LastStartBracket, LastEndBracket) = FindLastStyleBlock(text);

            // TODO If there was no style block, return the text with its parent's style.

            // If there is no style block in the text, return the text with the style of its parent text
            if (LastStartBracket < 0 || LastEndBracket < 0)
            {
                return new TextPart<Color, SpriteFont>[] { new TextPart<Color, SpriteFont>(text, parentColor, parentFont) };
            }

            // Extract styles
            var styleText = text.Substring(LastStartBracket + 1, LastEndBracket - LastStartBracket - 1);
            var styles = Regex.Replace(styleText, @"\s", "").Split(',');
            if (styles.Length == 0)
            {
                return new TextPart<Color, SpriteFont>[] { new TextPart<Color, SpriteFont>(text, parentColor, parentFont) };
            }

            // Try to extract styles
            try
            {
                var (Font, Color) = ExtractStyles(styles, parentFont, parentColor);

                #region Only style whatever is left of the style block
                // Split at style block. The reason for this is to only style text left to the style block
                var styleBlock = $"[{styleText}]";
                var styleBlockIndex = text.IndexOf(styleBlock);
                var leftPart = text.Substring(0, styleBlockIndex);

                // Check if there is a text part right of the style block
                int rightIndex = styleBlockIndex + styleBlock.Length + 1;
                bool rightPartExists = rightIndex < text.Length;

                // Add the text left of the style block and any right text if there is any
                var partCount = rightPartExists ? 2 : 1;
                var stylizedParts = new TextPart<Color, SpriteFont>[partCount];
                stylizedParts[0] = new TextPart<Color, SpriteFont>(leftPart, Color, Font);

                if (rightPartExists)
                {
                    var rightPart = text.Substring(rightIndex);
                    stylizedParts[1] = new TextPart<Color, SpriteFont>(rightPart, DefaultColor, DefaultFont);
                }
                #endregion

                return stylizedParts;
            }
            catch (Exception)
            {
            }

            // No styles found, return the original text
            return new TextPart<Color, SpriteFont>[] { new TextPart<Color, SpriteFont>(text, parentColor, parentFont) };
        }

        private (SpriteFont Font, Color Color) ExtractStyles(string[] styles, SpriteFont parentFont, Color parentColor)
        {
            var font = parentFont;
            var color = parentColor;

            foreach (var s in styles)
            {
                try
                {
                    var styleName = s.Substring(0, s.LastIndexOf('='));
                    var styleValue = s.Substring(styleName.Length + 1, s.Length - styleName.Length - 1);

                    if (Enum.TryParse(styleName.ToUpper(), out Style style))
                    {
                        switch (style)
                        {
                            case Style.COLOR:
                                var tempColor = GetColor(styleValue);
                                color = new Color(tempColor, tempColor.A);
                                break;
                            case Style.FONT:
                                if (content != null)
                                    font = content.Load<SpriteFont>(FontContentDirectory + styleValue);
                                break;
                            case Style.OPACITY:
                                color = new Color(color, float.Parse(styleValue));
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception)
                {
                    throw new Exception("Exception: Unidentifiable style!");
                }
            }

            return (font, color);
        }

        private Color GetColor(string colorName)
        {
            // There is not any color with no name
            if (colorName.Length == 0)
                return DefaultColor;

            // Change the color name to all lowercase
            colorName = colorName.ToLower();

            // Try to find an color with the same name

            foreach (var color in colorProperties)
            {
                if (color.Name.ToLower().Equals(colorName))
                {
                    return (Color)color.GetValue(null, null);
                }
            }

            throw new ArgumentException("A color with that name does not exist!");
        }

    }
}
