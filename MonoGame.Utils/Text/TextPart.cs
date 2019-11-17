using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGame.Utils.Text
{
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
