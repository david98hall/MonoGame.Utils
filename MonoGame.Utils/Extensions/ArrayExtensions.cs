namespace MonoGame.Utils.Extensions
{

    public static class ArrayExtensions
    {
        /// <summary>
        /// Interleaves this array with the specified one, 
        /// i.e. the elements are inserted in an alternating fashion.
        /// </summary>
        /// <typeparam name="T">The type of items in the arrays</typeparam>
        /// <param name="array1">This array</param>
        /// <param name="array2">The array to interleave with.</param>
        /// <returns></returns>
        public static T[] Interleave<T>(this T[] array1, T[] array2)
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
    }
}
