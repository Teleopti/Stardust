using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin.Models
{
	internal class FakeScheduleStorage_DoNotUse : IScheduleStorage
	{
		internal readonly IList<IPersistableScheduleData> _data = new List<IPersistableScheduleData>();

		public DateTimePeriod ThePeriodThatWasUsedForFindingSchedules { get; private set; }

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

		public IEnumerable<IPersistableScheduleData> LoadAll()
		{
			return _data;
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateTimePeriod dateTimePeriod, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateOnlyPeriod period, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(IEnumerable<IPerson> persons,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateOnlyPeriod period, IScenario scenario,
			bool dontTrackChangesForPersonAssignment = false)
		{
			throw new NotImplementedException();
		}

		public IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence)
		{
			ThePeriodThatWasUsedForFindingSchedules = period;

			var periods = _data.OfType<IPersonAbsence>()
				.Where(
					p =>
						p.Period.Intersect(period) && (absence == null || p.Layer.Payload.Equals(absence)) && p.Scenario.Equals(scenario) &&
						p.Person.Equals(person)).Select(s => s.Period);
			var absencesPeriod = !periods.Any() ? period : periods.Aggregate((a, b) => a.MaximumPeriod(b));

			var scheduleData = _data.Where(d => d.BelongsToScenario(scenario) && d.Period.Intersect(absencesPeriod)).ToArray();
			return ScheduleDictionaryForTest.WithScheduleData(person, scenario, absencesPeriod, data: scheduleData)[person];
		}

		public IScheduleDictionary FindSchedulesForPersons(IScenario scenario, IEnumerable<IPerson> peopleInOrg, ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateTimePeriod dateTimePeriod, IEnumerable<IPerson> visiblePersons, bool extendPeriodBasedOnVisiblePersons)
		{
			throw new NotImplementedException();
		}

		public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			throw new NotImplementedException();
		}

	}
}