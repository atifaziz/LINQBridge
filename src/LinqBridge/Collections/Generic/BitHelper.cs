using System;
using System.Security;

namespace System.Collections.Generic
{
	internal class BitHelper
	{
		private const byte MarkedBitFlag = 1;

		private const byte IntSize = 32;

		private int m_length;

		private unsafe int* m_arrayPtr;

		private int[] m_array;

		private bool useStackAlloc;

		[SecurityCritical]
		internal unsafe BitHelper(int* bitArrayPtr, int length)
		{
			this.m_arrayPtr = bitArrayPtr;
			this.m_length = length;
			this.useStackAlloc = true;
		}

		internal BitHelper(int[] bitArray, int length)
		{
			this.m_array = bitArray;
			this.m_length = length;
		}

		[SecurityCritical]
		internal bool IsMarked(int bitPosition)
		{
			unsafe
			{
				if (!this.useStackAlloc)
				{
					int num = bitPosition / 32;
					if (num >= this.m_length || num < 0)
					{
						return false;
					}
					return (this.m_array[num] & 1 << (bitPosition % 32 & 31)) != 0;
				}
				int num1 = bitPosition / 32;
				if (num1 >= this.m_length || num1 < 0)
				{
					return false;
				}
				return (*(this.m_arrayPtr + num1 * 4) & 1 << (bitPosition % 32 & 31)) != 0;
			}
		}

		[SecurityCritical]
		internal void MarkBit(int bitPosition)
		{
			unsafe
			{
				if (!this.useStackAlloc)
				{
					int num = bitPosition / 32;
					if (num < this.m_length && num >= 0)
					{
						ref int mArray = ref this.m_array[num];
						mArray = mArray | 1 << (bitPosition % 32 & 31);
					}
				}
				else
				{
					int num1 = bitPosition / 32;
					if (num1 < this.m_length && num1 >= 0)
					{
						int* mArrayPtr = this.m_arrayPtr + num1 * 4;
						*mArrayPtr = *mArrayPtr | 1 << (bitPosition % 32 & 31);
						return;
					}
				}
			}
		}

		internal static int ToIntArrayLength(int n)
		{
			if (n <= 0)
			{
				return 0;
			}
			return (n - 1) / 32 + 1;
		}
	}
}