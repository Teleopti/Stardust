using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
	public interface IAnalyticsScheduleRepository
	{
		void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows);
		void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount);
		void DeleteFactSchedule(int date, int personId, int scenarioId);
		IList<KeyValuePair<DateOnly, int>> Dates();

		IList<IAnalyticsActivity> Activities();
		IList<IAnalyticsAbsence> Absences();
		IList<IAnalyticsGeneric> Scenarios();
		IList<IAnalyticsGeneric> ShiftCategories();
		IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode);
		IList<IAnalyticsGeneric> Overtimes();
		IList<IAnalyticsShiftLength> ShiftLengths();
		int ShiftLengthId(int shiftLength);
	}
}