using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class TimeFilterInfo : IEquatable<TimeFilterInfo>
	{
		public IEnumerable<DateTimePeriod> StartTimes { get; set; }
		public IEnumerable<DateTimePeriod> EndTimes { get; set; } 
		public bool IsDayOff { get; set; }
		public bool IsEmptyDay { get; set; }
		public bool IsWorkingDay { get; set; }
		public bool OnlyNightShift { get; set; }

		#region			Equals

		public bool Equals(TimeFilterInfo other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.GetHashCode() == this.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(TimeFilterInfo)) return false;
			return Equals((TimeFilterInfo)obj);
		}

		public override int GetHashCode()
		{
			string StartTimesHash = (StartTimes == null) ? "NULL" : string.Join(",", StartTimes.Select(x => x.ToString()));
			string EndTimesHash = (EndTimes == null) ? "NULL" : string.Join(",", EndTimes.Select(x => x.ToString()));

			unchecked
			{
				return (StartTimesHash + EndTimesHash + IsDayOff.ToString() + IsEmptyDay.ToString() +
				 IsWorkingDay.ToString()).GetHashCode();
			}
		}

		#endregion
	}
}