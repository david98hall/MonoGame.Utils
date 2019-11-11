using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MonoGame.Utils.Text
{
    public partial class StylizedTextParser
    {
        #region Properties
        public SpriteFont DefaultFont { get; set; }
        public Color DefaultColor { get; set; }

        public float RowSpacing { get; set; }

        /// <summary>
        /// The character for declaring a new line.
        /// </summary>
        public char NewLine { get; set; }

        public char EscapeCharacter { get; set; }

        #endregion

        #region Fields

        // Content
        private readonly ContentManager content;
        public static string FontContentDirectory = "../Content/Fonts/";

        // Colors
        private readonly static IEnumerable<PropertyInfo> colorProperties =
            typeof(Color).GetProperties().Where(x => x.PropertyType == typeof(Color));
        #endregion

        /// <summary>
        /// Sets values.
        /// </summary>
        /// <param name="content">The content manager used to load fonts.</param>
        /// <param name="defaultFont">The default font if nothing else is specified.</param>
        /// <param name="defaultColor">The default color if nothing else is specified.</param>
        /// <param name="rowSpacing">The spacing between rows in parsed text.</param>
        /// <param name="newLine">The character for declaring a new line.</param>
        public StylizedTextParser(
            ContentManager content,
            SpriteFont defaultFont = null,
            Color defaultColor = default,
            float rowSpacing = 3,
            char newLine = '\n',
            char escapeCharacter = '\\')
        {
            this.content = content;
            DefaultFont = defaultFont;
            DefaultColor = defaultColor;
            RowSpacing = rowSpacing;
            NewLine = newLine;
            EscapeCharacter = escapeCharacter;
        }

        public enum Style
        {
            COLOR, FONT, OPACITY
        }

        public class Word
        {

            public string Text { get; set; }

            public SpriteFont Font { get; set; }

            public Color Color { get; set; }

            public Word(string text, SpriteFont font, Color color)
            {
                Text = text;
                Font = font;
                Color = color;
            }

        }

    }

}
