using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public IList<IPersistableScheduleData> LoadAll()
		{
			return _data;
		}

		public void SetUnitOfWork(IUnitOfWork unitOfWork)
		{
			UnitOfWork = unitOfWork;
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

			var scheduleData = _data
				.Where(d => d.BelongsToScenario(scenario))
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
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateOnlyPeriod period, IScenario scenario)
		{
			return FindSchedulesForPersonOnlyInGivenPeriod (person, scheduleDictionaryLoadOptions, period.ToDateTimePeriod (TimeZoneInfo.Utc), scenario);
		}

		public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(IEnumerable<IPerson> persons,
			IScheduleDictionaryLoadOptions
				scheduleDictionaryLoadOptions, DateOnlyPeriod period,
			IScenario scenario)
		{
			var thePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(period.StartDate, period.EndDate.AddDays(1), TimeZoneInfo.Utc);
			return ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario, thePeriod, _data.Where(d => d.BelongsToScenario(scenario)&& persons.Contains(d.Person)).ToArray());
		}

		public IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence = null)
		{
			ThePeriodThatWasUsedForFindingSchedules = period;

			var absencesPeriod =
				_data.OfType<IPersonAbsence>()
					.Where(
						p =>
							p.Period.Intersect(period) && (absence == null || p.Layer.Payload.Equals(absence)) && p.Scenario.Equals(scenario) &&
							p.Person.Equals(person)).Select(s => s.Period).Aggregate((a, b) => a.MaximumPeriod(b));

			var scheduleData = _data.Where(d => d.BelongsToScenario(scenario)).ToArray();
			return ScheduleDictionaryForTest.WithScheduleData(person, scenario, absencesPeriod, scheduleData)[person];
		}

		public IScheduleDictionary FindSchedulesForPersons(DateTimePeriod period, IScenario scenario, IPersonProvider personsProvider,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IEnumerable<IPerson> visiblePersons)
		{
			return null;
		}

		public IScheduleDictionary FindSchedulesForPersons(IScheduleDateTimePeriod period, IScenario scenario,
			IPersonProvider personsProvider,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			IEnumerable<IPerson> visiblePersons)
		{
			var dateTimePeriod = period.VisiblePeriod;
			var schedules = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var range = new FakeScheduleRange(schedules, new ScheduleParameters(scenario, visiblePersons.FirstOrDefault(), dateTimePeriod));
			var updatedRange = range.UpdateCalcValues(0, new TimeSpan());
			schedules.AddTestItem(visiblePersons.FirstOrDefault(), updatedRange);
			return schedules;
		}

		public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			throw new NotImplementedException();
		}
	}
}