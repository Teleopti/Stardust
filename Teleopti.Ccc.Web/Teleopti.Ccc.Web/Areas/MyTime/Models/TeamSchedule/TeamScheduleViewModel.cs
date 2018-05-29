using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TeamScheduleViewModel
	{
		public TeamScheduleAgentScheduleViewModel[] AgentSchedules { get; set; }
		public TeamScheduleTimeLineViewModel[] TimeLine { get; set; }
		public int PageCount { get; set; }
		public TeamScheduleAgentScheduleViewModel MySchedule { get; set; }
	}

	public class TeamScheduleAgentScheduleViewModel
	{
		public string Name { get; set; }
		public DateTime StartTimeUtc { get; set; }
		public Guid PersonId { get; set; }
		public DateTime? MinStart { get; set; }
		public int Total { get; set; }
		public string DayOffName { get; set; }
		public double ContractTimeInMinute { get; set; }

		public string Date { get; set; }
		public string FixedDate { get; set; }
		public HeaderViewModel Header { get; set; }
		public bool HasMainShift { get; set; }
		public bool HasOvertime { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public bool IsDayOff { get; set; }
		public TeamScheduleAgentScheduleLayerViewModel Summary { get; set; }
		public IEnumerable<TeamScheduleAgentScheduleLayerViewModel> Periods { get; set; }
		public bool HasNotScheduled { get; set; }
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
		public bool IsAbsenceConfidential { get; set; }
		public MeetingViewModel Meeting { get; set; }
	}
}