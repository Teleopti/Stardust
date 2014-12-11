using System;
using Teleopti.Interfaces.Infrastructure;
namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleDayCount : IAnalyticsFactScheduleDayCount
	{
		public int ShiftStartDateLocalId { get; set; }
		public int PersonId { get; set; }
		public int BusinessUnitId { get; set; }
		public int ScenarioId { get; set; }
		public int ShiftCategoryId { get; set; }
		public string DayOffName { get; set; }
		public string DayOffShortName { get; set; }
		public int AbsenceId { get; set; }
		public DateTime StartTime { get; set; }
	}
}