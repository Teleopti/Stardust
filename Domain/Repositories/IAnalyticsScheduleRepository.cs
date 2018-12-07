using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsScheduleRepository
	{
		void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows);
		void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount);
		void DeleteFactSchedules(IEnumerable<int> dateIds, Guid personCode, int scenarioId);
		
		int ShiftLengthId(int shiftLength);

		void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId,DateTime datasourceUpdateDate);

		void UpdateUnlinkedPersonids(int[] personPeriodIds);
		IList<IDateWithDuplicate> GetDuplicateDatesForPerson(Guid personCode);
		void RunWithExceptionHandling(Action action);
	}
}