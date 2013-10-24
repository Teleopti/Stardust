using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TeamScheduleViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public Guid TeamSelection { get; set; }
		public IEnumerable<AgentScheduleViewModel> AgentSchedules { get; set; }
		public IEnumerable<TimeLineViewModel> TimeLine { get; set; }
		public bool ShiftTradePermisssion { get; set; }
	}

	public class TimeLineViewModel
	{
		public string ShortTime { get; set; }
		public string LongTime { get; set; }
		public decimal PositionPercent { get; set; }
	}

	public class AgentScheduleViewModel
	{
		public bool HasDayOffUnder { get; set; }
		public string AgentName { get; set; }
		public IEnumerable<LayerViewModel> Layers { get; set; }
		public string DayOffText { get; set; }
	}

	public class LayerViewModel
	{
		public string Color { get; set; }
		public decimal PositionPercent { get; set; }
		public decimal EndPositionPercent { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string ActivityName { get; set; }
	}
}