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
		void DeleteFactSchedule(int dateId, int personId, int scenarioId);

		IList<IAnalyticsShiftLength> ShiftLengths();

		int ShiftLengthId(int shiftLength);

		void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId,DateTime datasourceUpdateDate);

		void UpdateUnlinkedPersonids(int[] personPeriodIds);
		int GetFactScheduleRowCount(int personId);
		int GetFactScheduleDayCountRowCount(int personId);
		int GetFactScheduleDeviationRowCount(int personId);
	}
}