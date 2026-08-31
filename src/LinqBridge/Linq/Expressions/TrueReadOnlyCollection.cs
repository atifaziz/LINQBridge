using System.Collections.ObjectModel;

namespace System.Linq.Expressions
{
    internal sealed class TrueReadOnlyCollection<T> : ReadOnlyCollection<T>
    {
        internal TrueReadOnlyCollection(T[] list) : base(list)
        {
        }
    }
}