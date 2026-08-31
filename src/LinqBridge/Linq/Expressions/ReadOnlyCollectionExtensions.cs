using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    internal static class ReadOnlyCollectionExtensions
    {
        internal static ReadOnlyCollection<T> ToReadOnlyCollection<T>(
            this IEnumerable<T> sequence)
        {
            if (sequence == null)
                return ReadOnlyCollectionExtensions.DefaultReadOnlyCollection<T>.Empty;
            return sequence is ReadOnlyCollection<T> readOnlyCollection ? readOnlyCollection : new ReadOnlyCollection<T>((IList<T>)sequence.ToArray<T>());
        }

        private static class DefaultReadOnlyCollection<T>
        {
            private static ReadOnlyCollection<T> _defaultCollection;

            internal static ReadOnlyCollection<T> Empty
            {
                get
                {
                    if (ReadOnlyCollectionExtensions.DefaultReadOnlyCollection<T>._defaultCollection == null)
                        ReadOnlyCollectionExtensions.DefaultReadOnlyCollection<T>._defaultCollection = new ReadOnlyCollection<T>((IList<T>)new T[0]);
                    return ReadOnlyCollectionExtensions.DefaultReadOnlyCollection<T>._defaultCollection;
                }
            }
        }
    }
}