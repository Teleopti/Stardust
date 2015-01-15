using System;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
	public interface IAnalyticsFactScheduleDayCount
	{
		int ShiftStartDateLocalId { get; set; }
		int PersonId { get; set; }
		int BusinessUnitId { get; set; }
		int ScenarioId { get; set; }
		int ShiftCategoryId { get; set; }
		string DayOffName { get; set; }
		string DayOffShortName { get; set; }
		int AbsenceId { get; set; }
		DateTime StartTime { get; set; }
	}
}