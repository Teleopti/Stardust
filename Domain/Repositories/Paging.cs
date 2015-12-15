using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class Paging : IEquatable<Paging>, IPaging
	{
		private int _totalCount;
		private int _take;
		private int _skip;

		public int Take
		{
			get { return _take; }
			set { _take = value; }
		}

		public int Skip
		{
			get { return _skip; }
			set { _skip = value; }
		}

		public int TotalCount
		{
			get { return _totalCount; }
			set { _totalCount = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly Paging Nothing = new Paging();

		#region			Equals

		public bool Equals(Paging other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.Take == Take && other.Skip == Skip && other.TotalCount == TotalCount;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (Paging)) return false;
			return Equals((Paging) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Take*397) ^ Skip ^ TotalCount;
			}
		}

		#endregion

	}
}