using System.Collections.Generic;

namespace MonoGame.Utils.Tuples
{
    public class UnorderedPair<T>
    {

        public readonly T T1;
        public readonly T T2;

        private readonly HashSet<T> parts;

        /// <summary>
        /// Sets the components in this pair.
        /// </summary>
        /// <param name="t1">The first element.</param>
        /// <param name="t2">The second element</param>
        public UnorderedPair(T t1, T t2)
        {
            T1 = t1;
            T2 = t2;
            parts = new HashSet<T> { T1, T2 };
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj != null && GetType() == obj.GetType())
            {
                UnorderedPair<T> otherPair = (UnorderedPair<T>)obj;
                return T1.Equals(otherPair.T1) && T2.Equals(otherPair.T2)
                    || T1.Equals(otherPair.T2) && T2.Equals(otherPair.T1);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return parts.GetHashCode();
        }

    }
}
