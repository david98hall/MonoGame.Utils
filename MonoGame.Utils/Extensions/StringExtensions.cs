namespace MonoGame.Utils.Text
{
    public static class StringExtensions
    {

        /// <summary>
        /// Counts the number of occurrences of the pattern in the text.
        /// </summary>
        /// <param name="text">The text to count in.</param>
        /// <param name="pattern">The pattern to look for.</param>
        /// <returns>The number of times the pattern was found in the text.</returns>
        public static int Count(this string text, string pattern)
        {
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

    }
}
