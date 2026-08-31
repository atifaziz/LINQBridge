using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public interface IOrderedQueryable : IQueryable, IEnumerable
    {
    }

    /// <summary>Represents the result of a sorting operation.</summary>
    /// <typeparam name="T">The type of the content of the data source.</typeparam>
    public interface IOrderedQueryable<T> :
        IQueryable<T>,
        IEnumerable<T>,
        IEnumerable,
        IQueryable,
        IOrderedQueryable
    {
    }
}