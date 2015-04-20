using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TeamScheduleViewModel
	{
		public PeriodSelectionViewModel PeriodSelection { get; set; }
		public Guid TeamSelection { get; set; }
		public IEnumerable<AgentScheduleViewModel> AgentSchedules { get; set; }
		public IEnumerable<TimeLineViewModel> TimeLine { get; set; }
		public bool ShiftTradePermisssion { get; set; }
		public bool ShiftTradeBulletinBoardPermission { get; set; }
	}

	public class PersonSchedule
	{
		public IPerson Person { get; set; }
		public IPersonScheduleDayReadModel Schedule { get; set; }
	}

	public class TeamScheduleViewModelReworked
	{
		
		public IEnumerable<AgentScheduleViewModelReworked> AgentSchedules { get; set; }		
		public IEnumerable<TimeLineViewModelReworked> TimeLine { get; set; }

		public int TimeLineLengthInMinutes { get; set; }
		public int PageCount { get; set; }
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
		public IEnumerable<LayerViewModel> Layers { get; set; }
		public string DayOffText { get; set; }
	}

	public class AgentScheduleViewModelReworked
	{		
		public IEnumerable<LayerViewModelReworked> ScheduleLayers { get; set; }
		public string Name { get; set; }
		public DateTime StartTimeUtc { get; set; }
		public Guid PersonId { get; set; }
		public DateTime? MinStart { get; set; }
		public bool IsDayOff { get; set; }
		public int Total { get; set; }	
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

	public class LayerViewModelReworked
	{
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int LengthInMinutes { get; set; }
		public string Color { get; set; }
		public string TitleHeader { get; set; }
		public bool IsAbsenceConfidential { get; set; }
		public string TitleTime { get; set; }
	}

}