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
		private readonly IAnalyticsPersonPeriodRepository _personPeriodRepository;
		public List<IFactScheduleRow> FactScheduleRows = new List<IFactScheduleRow>();
		public List<IAnalyticsFactScheduleDayCount> FactScheduleDayCountRows = new List<IAnalyticsFactScheduleDayCount>();
		public List<IAnalyticsShiftLength> AnalyticsShiftLengths = new List<IAnalyticsShiftLength>();

		public FakeAnalyticsScheduleRepository(IAnalyticsPersonPeriodRepository personPeriodRepository)
		{
			_personPeriodRepository = personPeriodRepository;
		}

		public void Has(IAnalyticsShiftLength shiftLength)
		{
			AnalyticsShiftLengths.Add(shiftLength);
		}

		public void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows)
		{
			FactScheduleRows.AddRange(factScheduleRows);
		}

		public void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount)
		{
			FactScheduleDayCountRows.Add(dayCount);
		}

		public void DeleteFactSchedule(int dateId, Guid personCode, int scenarioId)
		{
			var periods = _personPeriodRepository.GetPersonPeriods(personCode);
			FactScheduleRows.RemoveAll(x => x.PersonPart != null
											&& periods.Any(period => period.PersonId == x.PersonPart.PersonId)
											&& x.DatePart != null
											&& x.DatePart.ScheduleDateId == dateId
											&& x.TimePart != null
											&& x.TimePart.ScenarioId == scenarioId);

			FactScheduleDayCountRows.RemoveAll(x => periods.Any(period => period.PersonId == x.PersonId)
													&& x.ShiftStartDateLocalId == dateId 
													&& x.ScenarioId == scenarioId);
		}

		public IList<IAnalyticsShiftLength> ShiftLengths()
		{
			return AnalyticsShiftLengths;
		}

		public int ShiftLengthId(int shiftLength)
		{
			return AnalyticsShiftLengths.FirstOrDefault(item => item.ShiftLength == shiftLength)?.Id ?? 0;
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