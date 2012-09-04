using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceDayInput
	{
		public DateOnly Date { get; set; }
		public Guid PreferenceId { get; set; }

		public TimeSpan? EarliestStartTime { get; set; }
		public TimeSpan? LatestStartTime { get; set; }

		public TimeSpan? EarliestEndTime { get; set; }
		public TimeSpan? LatestEndTime { get; set; }

		public TimeSpan? MinimumWorkTime { get; set; }
		public TimeSpan? MaximumWorkTime { get; set; }

		public Guid ActivityPreferenceId { get; set; }

		public TimeSpan? ActivityMinimumTime { get; set; }
		public TimeSpan? ActivityMaximumTime { get; set; }

		public TimeSpan? ActivityEarliestStartTime { get; set; }
		public TimeSpan? ActivityLatestStartTime { get; set; }

		public TimeSpan? ActivityEarliestEndTime { get; set; }
		public TimeSpan? ActivityLatestEndTime { get; set; }
	}
}