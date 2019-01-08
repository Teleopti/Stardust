using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TeamScheduleViewModel
	{
		public TeamScheduleAgentScheduleViewModel[] AgentSchedules { get; set; }
		public TeamScheduleTimeLineViewModel[] TimeLine { get; set; }
		public int PageCount { get; set; }
		public TeamScheduleAgentScheduleViewModel MySchedule { get; set; }
		public int TotalAgentCount { get; set; }
	}

	public class TeamScheduleAgentScheduleViewModel
	{
		public string Name { get; set; }
		public IEnumerable<TeamScheduleAgentScheduleLayerViewModel> Periods { get; set; }
		public bool IsDayOff { get; set; }
		public string DayOffName { get; set; }
		public bool IsNotScheduled { get; set; }
		public ShiftCategoryViewModel ShiftCategory { get; set; }
		public DateTime BelongsToDate { get; set; }
	}

	public class TeamScheduleAgentScheduleLayerViewModel
	{
		public string Title { get; set; }
		public string TimeSpan { get; set; }
		public string Color { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool IsOvertime { get; set; }
		public decimal StartPositionPercentage { get; set; }
		public decimal EndPositionPercentage { get; set; }
		public MeetingViewModel Meeting { get; set; }
		public bool ShowMeetingIcon { get; set; }
	}
}