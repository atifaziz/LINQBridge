using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    internal static class EmptyReadOnlyCollection<T>
    {
        internal static ReadOnlyCollection<T> Instance;

        static EmptyReadOnlyCollection()
        {
            EmptyReadOnlyCollection<T>.Instance = new TrueReadOnlyCollection<T>(new T[0]);
        }
    }
}