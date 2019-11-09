using System.Collections.Generic;

namespace MonoGame.Utils.Tuples
{
    public class MutableTuple<T1, T2>
    {

        public T1 Item1 { get; set; }

        public T2 Item2 { get; set; }

        public MutableTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public override bool Equals(object obj)
        {
            return obj is MutableTuple<T1, T2> tuple &&
                   EqualityComparer<T1>.Default.Equals(Item1, tuple.Item1) &&
                   EqualityComparer<T2>.Default.Equals(Item2, tuple.Item2);
        }

        public override int GetHashCode()
        {
            var hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            return hashCode;
        }
    }
}
