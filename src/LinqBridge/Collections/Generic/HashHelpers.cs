using System.Runtime.ConstrainedExecution;

namespace System.Collections.Generic
{
    internal static class HashHelpers
    {
        internal static readonly int[] primes = new int[72]
        {
      3,
      7,
      11,
      17,
      23,
      29,
      37,
      47,
      59,
      71,
      89,
      107,
      131,
      163,
      197,
      239,
      293,
      353,
      431,
      521,
      631,
      761,
      919,
      1103,
      1327,
      1597,
      1931,
      2333,
      2801,
      3371,
      4049,
      4861,
      5839,
      7013,
      8419,
      10103,
      12143,
      14591,
      17519,
      21023,
      25229,
      30293,
      36353,
      43627,
      52361,
      62851,
      75431,
      90523,
      108631,
      130363,
      156437,
      187751,
      225307,
      270371,
      324449,
      389357,
      467237,
      560689,
      672827,
      807403,
      968897,
      1162687,
      1395263,
      1674319,
      2009191,
      2411033,
      2893249,
      3471899,
      4166287,
      4999559,
      5999471,
      7199369
        };

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static bool IsPrime(int candidate)
        {
            if ((candidate & 1) == 0)
                return candidate == 2;
            int num = (int)Math.Sqrt((double)candidate);
            for (int index = 3; index <= num; index += 2)
            {
                if (candidate % index == 0)
                    return false;
            }
            return true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static int GetPrime(int min)
        {
            for (int index = 0; index < HashHelpers.primes.Length; ++index)
            {
                int prime = HashHelpers.primes[index];
                if (prime >= min)
                    return prime;
            }
            for (int candidate = min | 1; candidate < int.MaxValue; candidate += 2)
            {
                if (HashHelpers.IsPrime(candidate))
                    return candidate;
            }
            return min;
        }

        internal static int GetMinPrime() => HashHelpers.primes[0];
    }
}
