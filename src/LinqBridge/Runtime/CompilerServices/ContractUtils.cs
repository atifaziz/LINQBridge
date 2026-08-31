using System.Collections.ObjectModel;

namespace System.Runtime.CompilerServices
{
    internal class ContractUtils
    {
        public static Exception Unreachable => throw new CompilerServicesException("Unreachable");

        public static void Requires(bool b, string s)
        {
            if (!b)
            {
                throw new ArgumentException(s);
            }
        }

        public static void RequiresNotEmpty<T>(ReadOnlyCollection<T> readOnly, string expressions)
        {
            if (readOnly.Count == 0)
            {
                throw new ArgumentException(expressions);
            }
        }

        public static void RequiresNotNull(object array, string s)
        {
            if (array is null)
            {
                throw new ArgumentException(s);
            }
        }

        public static void RequiresNotEmpty<T>(T[] readOnly, string expressions)
        {
            if (readOnly.Length == 0)
            {
                throw new ArgumentException(expressions);
            }
        }
    }
}