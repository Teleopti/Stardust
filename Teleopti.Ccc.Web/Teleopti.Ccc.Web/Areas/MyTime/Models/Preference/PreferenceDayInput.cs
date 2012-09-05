using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Preference
{
	public class PreferenceDayInput
	{
		public DateOnly Date { get; set; }
		public Guid PreferenceId { get; set; }

		public TimeOfDay? EarliestStartTime { get; set; }
		public TimeOfDay? LatestStartTime { get; set; }

		public TimeOfDay? EarliestEndTime { get; set; }
		public TimeOfDay? LatestEndTime { get; set; }

		public TimeSpan? MinimumWorkTime { get; set; }
		public TimeSpan? MaximumWorkTime { get; set; }

		public Guid ActivityPreferenceId { get; set; }

		public TimeSpan? ActivityMinimumTime { get; set; }
		public TimeSpan? ActivityMaximumTime { get; set; }

		public TimeOfDay? ActivityEarliestStartTime { get; set; }
		public TimeOfDay? ActivityLatestStartTime { get; set; }

		public TimeOfDay? ActivityEarliestEndTime { get; set; }
		public TimeOfDay? ActivityLatestEndTime { get; set; }
	}
}