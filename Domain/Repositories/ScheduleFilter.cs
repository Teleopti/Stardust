using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class ScheduleFilter : IEquatable<ScheduleFilter>
	{
		public string TeamIds { get; set; }

		public string FilteredStartTimes { get; set; }

		public string FilteredEndTimes { get; set; }

		public bool IsDayOff { get; set; }

		public bool IsEmptyDay { get; set; }

		public string SearchNameText { get; set; }

		public string TimeSortOrder { get; set; }


		#region			Equals

		public bool Equals(ScheduleFilter other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return this.GetHashCode() == other.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(ScheduleFilter)) return false;
			return Equals((ScheduleFilter)obj);
		}

		public override int GetHashCode()
		{
			var HashCodeForString = TeamIds + FilteredStartTimes + FilteredEndTimes + IsDayOff.ToString() + IsEmptyDay.ToString() + SearchNameText + TimeSortOrder;
			unchecked
			{
				return HashCodeForString.GetHashCode();
			}
		}

		#endregion

	}
}