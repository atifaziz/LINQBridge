using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Linq.Expressions
{
    public static class CollectionExtension
    {

        internal static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return EmptyReadOnlyCollection<T>.Instance;
            }
            TrueReadOnlyCollection<T> trueReadOnlyCollection = enumerable as TrueReadOnlyCollection<T>;
            if (trueReadOnlyCollection != null)
            {
                return trueReadOnlyCollection;
            }
            ReadOnlyCollectionBuilder<T> ts = enumerable as ReadOnlyCollectionBuilder<T>;
            if (ts != null)
            {
                return ts.ToReadOnlyCollection();
            }
            ICollection<T> ts1 = enumerable as ICollection<T>;
            if (ts1 == null)
            {
                return new TrueReadOnlyCollection<T>((new List<T>(enumerable)).ToArray());
            }
            int count = ts1.Count;
            if (count == 0)
            {
                return EmptyReadOnlyCollection<T>.Instance;
            }
            T[] tArray = new T[count];
            ts1.CopyTo(tArray, 0);
            return new TrueReadOnlyCollection<T>(tArray);
        }
    }
}
