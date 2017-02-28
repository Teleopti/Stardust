using System;

namespace Teleopti.Interfaces.Domain
{
	public struct Paging : IEquatable<Paging>
	{
		public int Take { get; set; }

		public int Skip { get; set; }

		public int TotalCount { get; set; }
		
		public static readonly Paging Empty = new Paging();

		#region			Equals

		public bool Equals(Paging other)
		{
			return other.Take == Take && other.Skip == Skip && other.TotalCount == TotalCount;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != typeof(Paging)) return false;
			return Equals((Paging)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Take * 397) ^ Skip ^ TotalCount;
			}
		}

		#endregion

	}
}