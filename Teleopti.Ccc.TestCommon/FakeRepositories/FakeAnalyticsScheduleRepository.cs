using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsScheduleRepository : IAnalyticsScheduleRepository
	{
		public List<IFactScheduleRow> FactScheduleRows = new List<IFactScheduleRow>();
		public List<IAnalyticsFactScheduleDayCount> FactScheduleDayCountRows = new List<IAnalyticsFactScheduleDayCount>();

		public void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows)
		{
			FactScheduleRows.AddRange(factScheduleRows);
		}

		public void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount)
		{
			FactScheduleDayCountRows.Add(dayCount);
		}

		public void DeleteFactSchedule(int dateId, int personId, int scenarioId)
		{
			FactScheduleRows.RemoveAll(x => x.PersonPart != null
											&& x.PersonPart.PersonId == personId
											&& x.DatePart != null
											&& x.DatePart.ScheduleDateId == dateId
											&& x.TimePart != null
											&& x.TimePart.ScenarioId == scenarioId);

			FactScheduleDayCountRows.RemoveAll(x => x.PersonId == personId
													&& x.ShiftStartDateLocalId == dateId 
													&& x.ScenarioId == scenarioId);
		}

		public IList<IAnalyticsShiftLength> ShiftLengths()
		{
			return new List<IAnalyticsShiftLength>();
		}

		public int ShiftLengthId(int shiftLength)
		{
			return 0;
		}

		public void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId,
			DateTime datasourceUpdateDate)
		{
			
		}

		public void UpdateUnlinkedPersonids(int[] personPeriodIds)
		{
			throw new NotImplementedException();
		}

		public int GetFactScheduleRowCount(int personId)
		{
			return FactScheduleRows.Count(x => x.PersonPart != null && x.PersonPart.PersonId == personId);
		}

		public int GetFactScheduleDayCountRowCount(int personId)
		{
			return FactScheduleDayCountRows.Count(x => x.PersonId == personId);
		}

		public int GetFactScheduleDeviationRowCount(int personId)
		{
			throw new NotImplementedException();
		}
	}
}