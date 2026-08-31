using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Threading
{
    public static class Net20Interlocked
    {
        static object lockObj = new object();

        [__DynamicallyInvokable]
        [ComVisible(false)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SecuritySafeCritical]
        /// <summary>
        /// Compares two instances for reference equality and, if they are equal,
        /// replaces the first one. Mirrors the contract of
        /// <see cref="Interlocked.CompareExchange{T}(ref T,T,T)"/>: comparison is by
        /// reference (so a <see langword="null" /> <paramref name="location1" /> is
        /// legal) and the value returned is the <em>original</em> value of
        /// <paramref name="location1" />, whether or not the exchange took place.
        /// </summary>
        public static T CompareExchange<T>(ref T location1, T value, T comparand)
        {
            lock (lockObj)
            {
                T original = location1;
                if (ReferenceEquals(original, comparand))
                {
                    location1 = value;
                }
                return original;
            }
        }

    }
}
