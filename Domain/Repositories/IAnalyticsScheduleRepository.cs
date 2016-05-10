using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsScheduleRepository
	{
		void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows);
		void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount);
		void DeleteFactSchedule(int date, int personId, int scenarioId);

		IList<IAnalyticsActivity> Activities();
		IList<IAnalyticsAbsence> Absences();
		IList<IAnalyticsGeneric> ShiftCategories();
		IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode);
		IList<IAnalyticsGeneric> Overtimes();
		IList<IAnalyticsShiftLength> ShiftLengths();

		int ShiftLengthId(int shiftLength);

		void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId,
			DateTime datasourceUpdateDate);
	}
}