using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public struct GuidCombinationKey : IEquatable<GuidCombinationKey>
	{
		public Guid[] First { get; }

		public GuidCombinationKey(Guid[] first)
		{
			First = first;
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

		public bool Equals(GuidCombinationKey other)
		{
			return compareArray(First, other.First);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is GuidCombinationKey && Equals((GuidCombinationKey)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				if (First == null)
				{
					return 0;
				}
				int hash = 17;
				foreach (var element in First)
				{
					hash = hash * 31 + element.GetHashCode();
				}
				return hash;
			}
		}
	}
}