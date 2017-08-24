using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleStorage : IScheduleStorage
	{
		private readonly IList<IPersistableScheduleData> _data = new List<IPersistableScheduleData>();

		public DateTimePeriod ThePeriodThatWasUsedForFindingSchedules { get; private set; }

		public void Add(IPersistableScheduleData entity)
		{
			_data.Add (entity);
		}

		public void Remove(IPersistableScheduleData entity)
		{
			_data.Remove (entity);
		}

		public IPersistableScheduleData Get(Type concreteType, Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersistableScheduleData> LoadAll()
		{
			return _data;
		}

	public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateTimePeriod dateTimePeriod, IScenario scenario)
		{
			ThePeriodThatWasUsedForFindingSchedules = dateTimePeriod;

			var scheduleData = _data
				.Where(d => d.BelongsToScenario(scenario))
				.Where(d => d.Person.Equals(person))
				.Where(d =>
				{
					var maybePersonAssignment = d as PersonAssignment;
					var maybePersonAbsence = d as PersonAbsence;

					if (maybePersonAssignment != null)
					{
						return dateTimePeriod.Contains(maybePersonAssignment.Period.StartDateTime);
					}

					if (maybePersonAbsence != null)
					{
						return maybePersonAbsence.Period.ContainsPart(dateTimePeriod);
					}

					return true;
				})				
				.ToArray();

			if (scheduleData.IsEmpty())
			{
				return ScheduleDictionaryForTest.WithScheduleData(person, scenario, dateTimePeriod, scheduleData); ;
			}
			var period = scheduleData.Select(s => s.Period).Aggregate((a, b) => a.MaximumPeriod(b));
			return ScheduleDictionaryForTest.WithScheduleData(person, scenario, period, scheduleData);
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateOnlyPeriod period, IScenario scenario)
		{
			return FindSchedulesForPersonOnlyInGivenPeriod (person, scheduleDictionaryLoadOptions, period.ToDateTimePeriod (TimeZoneInfo.Utc), scenario);
		}

		public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(IEnumerable<IPerson> persons,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateOnlyPeriod period, IScenario scenario)
		{
			var thePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(period.StartDate, period.EndDate.AddDays(1), TimeZoneInfo.Utc);
			return ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario, thePeriod, _data.Where(d => d.BelongsToScenario(scenario)&& persons.Contains(d.Person)).ToArray());
		}

		public IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence = null)
		{
			ThePeriodThatWasUsedForFindingSchedules = period;

			var periods = _data.OfType<IPersonAbsence>()
				.Where(
					p =>
						p.Period.Intersect(period) && (absence == null || p.Layer.Payload.Equals(absence)) && p.Scenario.Equals(scenario) &&
						p.Person.Equals(person)).Select(s => s.Period);
			var absencesPeriod = !periods.Any() ? period : periods.Aggregate((a, b) => a.MaximumPeriod(b));

			var scheduleData = _data.Where(d => d.BelongsToScenario(scenario) && d.Period.Intersect(absencesPeriod)).ToArray();
			return ScheduleDictionaryForTest.WithScheduleData(person, scenario, absencesPeriod, scheduleData)[person];
		}

		public IScheduleDictionary FindSchedulesForPersons(IScheduleDateTimePeriod period, IScenario scenario,
			IPersonProvider personsProvider,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			IEnumerable<IPerson> visiblePersons)
		{
			var dateTimePeriod = period.LongLoadedDateOnlyPeriod().ToDateTimePeriod(TimeZoneInfo.Utc);
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			if (_data == null || !_data.Any())
			{
				return schedules;
			}

			foreach (var visiblePerson in visiblePersons)
			{
				var range = new ScheduleRange(schedules, new ScheduleParameters(scenario, visiblePerson, dateTimePeriod),new ByPassPersistableScheduleDataPermissionChecker());
				foreach (var scheduleData in _data)
				{
					if (scheduleData.Person == null || !scheduleData.Person.Equals(range.Person))
						continue;
					if(scheduleData.Period.Intersect(dateTimePeriod))
						range.Add(scheduleData);
				}
				//var updatedRange = range.UpdateCalcValues(0, new TimeSpan());
				schedules.AddTestItem(visiblePerson, range);
			}
			schedules.TakeSnapshot();
			return schedules;
		}

		public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			throw new NotImplementedException();
		}
	}
}