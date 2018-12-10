using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Exceptions;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;


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

		private void deleteFactSchedule(int dateId, Guid personCode, int scenarioId)
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

		public void DeleteFactSchedules(IEnumerable<int> dateIds, Guid personCode, int scenarioId)
		{
			foreach (var dateId in dateIds)
			{
				deleteFactSchedule(dateId, personCode, scenarioId);
			}
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

		public IList<IDateWithDuplicate> GetDuplicateDatesForPerson(Guid personCode)
		{
			//var periods = _personPeriodRepository.GetPersonPeriods(personCode);
			//FactScheduleRows.GroupBy(fs => fs.DatePart.ScheduleStartDateLocalId).Where(fs => fs.)
			throw new NotImplementedException();
		}

		public void RunWithExceptionHandling(Action action)
		{
			try
			{
				action();
			}
			catch (ConstraintViolationException e)
			{
				throw new ConstraintViolationWrapperException(e);
			}
		}
	}
}