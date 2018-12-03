using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Core.Data;


namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TeamScheduleViewModelToggle75989Off
	{
		public AgentInTeamScheduleViewModel[] AgentSchedules { get; set; }
		public TeamScheduleTimeLineViewModelToggle75989Off[] TimeLine { get; set; }
		public int PageCount { get; set; }
		public AgentInTeamScheduleViewModel MySchedule { get; set; }
	}

	public class PersonSchedule
	{
		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
		public IPersonScheduleDayReadModel Schedule { get; set; }
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
		public double ContractTimeInMinute { get; set; }
		public bool IsNotScheduled { get; set; }
		public ShiftCategoryViewModel ShiftCategory { get; set; }
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