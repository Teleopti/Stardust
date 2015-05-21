using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleDataReadScheduleRepository : IScheduleRepository
	{

		public void InitRangeValues(int targetDaysOff, int scheduledDaysOff, TimeSpan targetContractTime, TimeSpan scheduledContractTime)
		{
			_targetDaysOff = targetDaysOff;
			_scheduledDaysOff = scheduledDaysOff;
			_targetContractTime = targetContractTime;
			_scheduledContractTime = scheduledContractTime;
		}
		private IEnumerable<IScheduleData> _data = new List<IScheduleData>();
		private int _targetDaysOff;
		private int _scheduledDaysOff;
		private TimeSpan _targetContractTime;
		private TimeSpan _scheduledContractTime;

		public DateTimePeriod ThePeriodThatWasUsedForFindingSchedules { get; private set; }

		public FakeScheduleDataReadScheduleRepository(params IScheduleData[] data)
		{
			_data = data;
		}

		public void Add(IPersistableScheduleData entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(IPersistableScheduleData entity)
		{
			throw new NotImplementedException();
		}

		public IPersistableScheduleData Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPersistableScheduleData> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPersistableScheduleData Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPersistableScheduleData> entityCollection)
		{
			entityCollection.ForEach (Add);
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IPersistableScheduleData Get(Type concreteType, Guid id)
		{
			throw new NotImplementedException();
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
		                                                                   IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
		                                                                   DateTimePeriod dateTimePeriod, IScenario scenario)
		{
			ThePeriodThatWasUsedForFindingSchedules = dateTimePeriod;

			var period = _data.First().Period; // max period?
			return ScheduleDictionaryForTest.WithScheduleData(person, scenario, period, _data.ToArray());
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateOnlyPeriod period, IScenario scenario)
		{
			return ScheduleDictionaryForTest.WithScheduleData(person, scenario, period.ToDateTimePeriod(TimeZoneInfo.Utc), _data.ToArray());
		}

		public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(IEnumerable<IPerson> persons,
		                                                                    IScheduleDictionaryLoadOptions
			                                                                    scheduleDictionaryLoadOptions, DateOnlyPeriod period,
		                                                                    IScenario scenario)
		{
			var thePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(period.StartDate, period.EndDate, TimeZoneInfo.Utc);
			return ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario, thePeriod, _data.ToArray());
		}

		public IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence)
		{
			throw new NotImplementedException();
		}

		public IScheduleDictionary FindSchedulesForPersons(IScheduleDateTimePeriod period, IScenario scenario,
		                                                   IPersonProvider personsProvider,
		                                                   IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
		                                                   IEnumerable<IPerson> visiblePersons)
		{
			var dateTimePeriod = period.VisiblePeriod;
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var range = new ScheduleRange(schedules, new ScheduleParameters(scenario, visiblePersons.FirstOrDefault(), dateTimePeriod));
			range.CalculatedTargetScheduleDaysOff = _scheduledDaysOff;
			schedules.AddTestItem(visiblePersons.FirstOrDefault(), range);
			return schedules;
		}

		public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			throw new NotImplementedException();
		}

		public void Set(IEnumerable<IScheduleData> data)
		{
			_data = data;
		}
	}
}