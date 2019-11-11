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
        private IEnumerable<(IEnumerable<Word> RowText, MutableTuple<float, float> RowSize)>
            FitTextHorizontally(
                IEnumerable<(IEnumerable<Word> RowText, MutableTuple<float, float> RowSize)> rows,
                float maxWidth)
        {
            var newRows =
                new List<(IEnumerable<Word> RowText, MutableTuple<float, float> RowSize)>(
                    rows.Count())
                {
                    (new LinkedList<Word>(), new MutableTuple<float, float>(0, 0))
                };

            // Build rows
            float rowWidth = 0;
            foreach (var (RowText, RowSize) in rows)
            {
                foreach (var word in RowText)
                {
                    if (word.Text.Length > 0)
                    {
                        var wordWidth = word.Font.MeasureString(word.Text).X;
                        var wordText = word.Text;
                        var newRowWidth = rowWidth + wordWidth;

                        if (newRowWidth > maxWidth)
                        {
                            rowWidth = wordWidth;
                            newRows.Add((new LinkedList<Word>(),
                                new MutableTuple<float, float>(0, 0)));
                        }
                        else
                        {
                            rowWidth = newRowWidth;
                        }


                        var words = newRows.Last().RowText as LinkedList<Word>;
                        words.AddLast(word);
                    }
                }
            }

            // Calculate row sizes
            for (int i = 0; i < newRows.Count; i++)
            {
                var (RowText, RowSize) = newRows[i];

                TrimRow(RowText as LinkedList<Word>);

                var rowSize = GetRowSize(RowText);
                RowSize.Item1 = rowSize.Item1;
                RowSize.Item2 = rowSize.Item2;

                // If the row is empty, remove it from the text
                if (rowSize.Item1 <= 0)
                {
                    newRows.RemoveAt(i);
                }

            }

            return newRows;
        }

        private void TrimRow(LinkedList<Word> row)
        {
            TrimRow(row, true);
            TrimRow(row, false);
        }

        private void TrimRow(LinkedList<Word> row, bool fromLeft)
        {
            if (row.Count > 0)
            {
                var endWord = new Word("", DefaultFont, DefaultColor);
                Word currentWord;
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

                        endWord = new Word(wordText, currentWord.Font, currentWord.Color);

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

        private IEnumerable<Word> ParseWords(Word word)
        {
            var stylizedWords = new List<Word>();

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
                    var rightPart = ParseWords(new Word(word.Text.Substring(i + 1), word.Font, word.Color));

                    // Extract the style of the first word to the right of the braced area 
                    // since the left side will have it as its default style
                    // var (_, FirstRightFont, FirstRightColor) = rightPart.First();
                    var rightPartWord = rightPart.First();

                    // Parse the words to the left of the braced area with the extracted style as the default
                    var leftWord = new Word(word.Text.Substring(0, latestStartBrace),
                        rightPartWord.Font,
                        rightPartWord.Color);
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

        private IEnumerable<Word> ParseStyle(string text, SpriteFont parentFont, Color parentColor)
        {
            // Try to look for the last style block
            var (LastStartBracket, LastEndBracket) = FindLastStyleBlock(text);

            // TODO If there was no style block, return the text with its parent's style.

            // If there is no style block in the text, return the text with the style of its parent text
            if (LastStartBracket < 0 || LastEndBracket < 0)
            {
                return new Word[] { new Word(text, parentFont, parentColor) };
            }

            // Extract styles
            var styleText = text.Substring(LastStartBracket + 1, LastEndBracket - LastStartBracket - 1);
            var styles = Regex.Replace(styleText, @"\s", "").Split(',');
            if (styles.Length == 0)
            {
                return new Word[] { new Word(text, parentFont, parentColor) };
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
                var stylizedParts = new Word[partCount];
                stylizedParts[0] = new Word(leftPart, Font, Color);

                if (rightPartExists)
                {
                    var rightPart = text.Substring(rightIndex);
                    stylizedParts[1] = new Word(rightPart, DefaultFont, DefaultColor);
                }
                #endregion

                return stylizedParts;
            }
            catch (Exception)
            {
            }

            // No styles found, return the original text
            return new Word[] { new Word(text, parentFont, parentColor) };
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
