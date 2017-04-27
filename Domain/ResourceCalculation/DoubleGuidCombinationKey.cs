using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public struct DoubleGuidCombinationKey : IEquatable<DoubleGuidCombinationKey>
	{
		public Guid[] First { get; }
		public Guid[] Second { get; }
		
		public DoubleGuidCombinationKey(Guid[] first, Guid[] second)
		{
			First = first;
			Second = second;
		}

		private bool compareArray(Guid[] first, Guid[] second)
		{
			if (first == second)
			{
				return true;
			}
			if (first == null || second == null)
			{
				return false;
			}
			if (first.Length != second.Length)
			{
				return false;
			}
			for (int i = 0; i < first.Length; i++)
			{
				if (first[i] != second[i])
				{
					return false;
				}
			}
			return true;
		}

		public bool Equals(DoubleGuidCombinationKey other)
		{
			return compareArray(First, other.First) && compareArray(Second, other.Second);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is DoubleGuidCombinationKey && Equals((DoubleGuidCombinationKey) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				if (First == null && Second == null) 
				{
					return 0;
				}
				int hash = 17;
				if (First != null)
				{
					foreach (var element in First)
					{
						hash = hash * 31 + element.GetHashCode();
					}
				}
				if (Second != null)
				{
					foreach (var element in Second)
					{
						hash = hash * 37 + element.GetHashCode();
					}
				}
				return hash;
			}
		}
	}
}