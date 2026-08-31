using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace System
{
    internal static class ArrayEx
    {
        /// <summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the first element and pastes them into another <see cref="T:System.Array" /> starting at the first element. The length is specified as a 32-bit integer.</summary>
        /// <param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///         <paramref name="sourceArray" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"/>
        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static T[] Copy<T>(this T[] sourceArray)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            Type t = sourceArray.GetType();
            Type t2 = t.GetElementType();
            var destinationArray= new T[ sourceArray.Length];
            int length = sourceArray.Length;
            Array.Copy(sourceArray, sourceArray.GetLowerBound(0), destinationArray, destinationArray.GetLowerBound(0), length);
            return destinationArray;
        }

        /// <summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the first element and pastes them into another <see cref="T:System.Array" /> starting at the first element. The length is specified as a 32-bit integer.</summary>
        /// <param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
        /// <param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
        /// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///         <paramref name="sourceArray" /> is <see langword="null" />.
        /// -or-
        /// <paramref name="destinationArray" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.RankException">
        /// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
        /// <exception cref="T:System.ArrayTypeMismatchException">
        /// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
        /// <exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="length" /> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///         <paramref name="length" /> is greater than the number of elements in <paramref name="sourceArray" />.
        /// -or-
        /// <paramref name="length" /> is greater than the number of elements in <paramref name="destinationArray" />.</exception>
        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static T[] Copy<T>(this T[] sourceArray, T[] destinationArray, int length)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray));
            if (destinationArray == null)
                throw new ArgumentNullException(nameof(destinationArray));
            Array.Copy(sourceArray, sourceArray.GetLowerBound(0), destinationArray, destinationArray.GetLowerBound(0), length);
            return destinationArray;
        }

        /// <summary>Copies a range of elements from an <see cref="T:System.Array" /> starting at the specified source index and pastes them to another <see cref="T:System.Array" /> starting at the specified destination index. The length and the indexes are specified as 32-bit integers.</summary>
        /// <param name="sourceArray">The <see cref="T:System.Array" /> that contains the data to copy.</param>
        /// <param name="sourceIndex">A 32-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
        /// <param name="destinationArray">The <see cref="T:System.Array" /> that receives the data.</param>
        /// <param name="destinationIndex">A 32-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
        /// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///         <paramref name="sourceArray" /> is <see langword="null" />.
        /// -or-
        /// <paramref name="destinationArray" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.RankException">
        /// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
        /// <exception cref="T:System.ArrayTypeMismatchException">
        /// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
        /// <exception cref="T:System.InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///         <paramref name="sourceIndex" /> is less than the lower bound of the first dimension of <paramref name="sourceArray" />.
        /// -or-
        /// <paramref name="destinationIndex" /> is less than the lower bound of the first dimension of <paramref name="destinationArray" />.
        /// -or-
        /// <paramref name="length" /> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///         <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />.
        /// -or-
        /// <paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
        [ReliabilityContract(Consistency.MayCorruptInstance, Cer.MayFail)]
        public static T[] Copy<T>(this T[] sourceArray,
          int sourceIndex,
          T[] destinationArray,
          int destinationIndex,
          int length)
        {
            Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
            return destinationArray;
        }

    }
}
