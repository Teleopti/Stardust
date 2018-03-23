using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsScheduleRepository
	{
		void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows);
		void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount);
		[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_48769)]
		void DeleteFactSchedule(int dateId, Guid personCode, int scenarioId);
		void DeleteFactSchedules(IEnumerable<int> dateIds, Guid personCode, int scenarioId);
		
		int ShiftLengthId(int shiftLength);

		void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId,DateTime datasourceUpdateDate);

		void UpdateUnlinkedPersonids(int[] personPeriodIds);
		int GetFactScheduleRowCount(int personId);
		int GetFactScheduleDayCountRowCount(int personId);
		int GetFactScheduleDeviationRowCount(int personId);
		IList<IDateWithDuplicate> GetDuplicateDatesForPerson(Guid personCode);
		void RunWithExceptionHandling(Action action);
	}
}