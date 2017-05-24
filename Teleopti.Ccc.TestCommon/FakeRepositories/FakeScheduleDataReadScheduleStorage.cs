using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleDataReadScheduleStorage : IScheduleStorage
	{
		public void InitRangeValues(int targetScheduledDaysOff, int scheduledDaysOff, TimeSpan targetTimeHolder, TimeSpan contractTimeHolder)
		{
			_scheduleDaysOff = scheduledDaysOff;
			_contractTimeHolder = contractTimeHolder;
		}

		private IList<IScheduleData> _data = new List<IScheduleData>();
		private int _scheduleDaysOff;
		private TimeSpan _contractTimeHolder;

		public DateTimePeriod ThePeriodThatWasUsedForFindingSchedules { get; private set; }

		public FakeScheduleDataReadScheduleStorage(params IScheduleData[] data)
		{
			data.ForEach(_data.Add);
		}

		public void Add(IPersistableScheduleData entity)
		{
			_data.Add(entity);
		}

		public void Remove(IPersistableScheduleData entity)
		{
			_data.Remove(entity);
		}

		public IPersistableScheduleData Get(Type concreteType, Guid id)
		{
			throw new NotImplementedException();
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
																		   ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
																		   DateTimePeriod dateTimePeriod, IScenario scenario)
		{
			if (!_data.Any())
			{
				return ScheduleDictionaryForTest.WithScheduleData(person, scenario, dateTimePeriod);
			}
			ThePeriodThatWasUsedForFindingSchedules = dateTimePeriod;

			var period = _data.First().Period; // max period?
			return ScheduleDictionaryForTest.WithScheduleData(person, scenario, period, _data.ToArray());
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateOnlyPeriod period, IScenario scenario)
		{
			var dateTimePeriod = period.ToDateTimePeriod(TimeZoneInfo.Utc);
			return ScheduleDictionaryForTest.WithScheduleData(person, scenario, dateTimePeriod, _data.Where(x => dateTimePeriod.Contains(x.Period.StartDateTime)).ToArray());
		}

		public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(IEnumerable<IPerson> persons,
																			ScheduleDictionaryLoadOptions
																				scheduleDictionaryLoadOptions, DateOnlyPeriod period,
																			IScenario scenario)
		{
			var thePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(period.StartDate, period.EndDate, TimeZoneInfo.Utc);
			return ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario, thePeriod, _data.ToArray());
		}

		public IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence)
		{
			var dict = ScheduleDictionaryForTest.WithScheduleData(person, scenario, period, _data.ToArray());
			return new FakeScheduleRange(dict, new ScheduleParameters(scenario, person, period));
		}

		public IScheduleDictionary FindSchedulesForPersons(IScheduleDateTimePeriod period, IScenario scenario,
														   IPersonProvider personsProvider,
														   ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
														   IEnumerable<IPerson> visiblePersons)
		{
			var dateTimePeriod = period.VisiblePeriod;
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);

			if (_data.Any())
			{
				schedules.AddScheduleDataManyPeople(_data.ToArray());
			}
			else
			{
				var range = new FakeScheduleRange(schedules, new ScheduleParameters(scenario, visiblePersons.FirstOrDefault(), dateTimePeriod));
				var updatedRange = range.UpdateCalcValues(_scheduleDaysOff, _contractTimeHolder);
				schedules.AddTestItem(visiblePersons.FirstOrDefault(), updatedRange);

			}

			return schedules;
		}

		public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			throw new NotImplementedException();
		}

		public void Set(IList<IScheduleData> data)
		{
			data.ForEach(_data.Add);
		}

		public void Clear()
		{
			_data.Clear();
		}
	}
}