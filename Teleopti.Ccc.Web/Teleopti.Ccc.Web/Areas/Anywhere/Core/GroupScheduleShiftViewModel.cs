using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GroupScheduleShiftViewModel
	{
		public string PersonId { get; set; }
		public string Name { get; set; }
		public string Date { get; set; }
		public double WorkTimeMinutes { get; set; }
		public double ContractTimeMinutes { get; set; }

		public IEnumerable<GroupScheduleProjectionViewModel> Projection { get; set; }
		public List<Guid> MultiplicatorDefinitionSetIds { get; set; }

		public ShiftCategoryViewModel ShiftCategory { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public GroupScheduleDayOffViewModel DayOff { get; set; }
		public string InternalNotes { get; set; }
		public string PublicNotes { get; set; }
		public TimeZoneViewModel Timezone { get; set; }
		public UnderlyingScheduleSummary UnderlyingScheduleSummary { get; set; }
	}

	public class UnderlyingScheduleSummary
	{
		public Summary[] PersonalActivities { get; set; }
		public Summary[] PersonPartTimeAbsences { get; set; }
		public Summary[] PersonMeetings { get; set; }
	}

	public class Summary
	{
		public string Description { get; set; }
		public string Start { get; set; }
		public string End { get; set; }
	}

	public class TimeZoneViewModel
	{
		public string IanaId { get; set; }
		public string DisplayName { get; set; }
	}
}