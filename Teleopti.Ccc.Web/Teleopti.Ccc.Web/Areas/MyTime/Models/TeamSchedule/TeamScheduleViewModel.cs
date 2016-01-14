using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TeamScheduleViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public Guid TeamSelection { get; set; }
		public AgentScheduleViewModel[] AgentSchedules { get; set; }
		public TimeLineViewModel[] TimeLine { get; set; }
		public bool ShiftTradePermisssion { get; set; }
		public bool ShiftTradeBulletinBoardPermission { get; set; }
	}

	public class PersonSchedule
	{
		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
		public IPersonScheduleDayReadModel Schedule { get; set; }
	}

	public class TeamScheduleViewModelReworked
	{
		
		public AgentInTeamScheduleViewModel[] AgentSchedules { get; set; }		
		public TimeLineViewModelReworked[] TimeLine { get; set; }

		public int TimeLineLengthInMinutes { get; set; }
		public int PageCount { get; set; }
		public AgentInTeamScheduleViewModel MySchedule { get; set; }
	}


	public class TimeLineViewModel
	{
		public string ShortTime { get; set; }
		public decimal PositionPercent { get; set; }
		public bool IsFullHour { get; set; }
	}

	public class TimeLineViewModelReworked
	{
		public string HourText { get; set; }
		public int LengthInMinutesToDisplay { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}


	public class AgentScheduleViewModel
	{
		public bool HasDayOffUnder { get; set; }
		public string AgentName { get; set; }
		public LayerViewModel[] Layers { get; set; }
		public string DayOffText { get; set; }
	}

	public class AgentInTeamScheduleViewModel
	{
		public TeamScheduleLayerViewModel[] ScheduleLayers { get; set; }
		public string Name { get; set; }
		public DateTime StartTimeUtc { get; set; }
		public Guid PersonId { get; set; }
		public DateTime? MinStart { get; set; }
		public bool IsDayOff { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public int Total { get; set; }
		public string DayOffName { get; set; }
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