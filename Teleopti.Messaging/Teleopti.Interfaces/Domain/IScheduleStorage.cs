using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
	public interface IScheduleStorage
	{

		IUnitOfWork UnitOfWork { get; }
		void Add(IPersistableScheduleData scheduleData);
		void Remove(IPersistableScheduleData scheduleData);
		IPersistableScheduleData Get(Type concreteType, Guid id);

		IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(
			IPerson person,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateTimePeriod dateTimePeriod,
			IScenario scenario);

		IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(
			IPerson person,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateOnlyPeriod period,
			IScenario scenario);

		IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(
			IEnumerable<IPerson> persons,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateOnlyPeriod period,
			IScenario scenario);

		IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person,
			IAbsence absence = null);

		IScheduleDictionary FindSchedulesForPersons(
			DateTimePeriod period,
			IScenario scenario,
			IPersonProvider personsProvider,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			IEnumerable<IPerson> visiblePersons);

		IScheduleDictionary FindSchedulesForPersons(
			IScheduleDateTimePeriod period,
			IScenario scenario,
			IPersonProvider personsProvider,
			IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			IEnumerable<IPerson> visiblePersons);

		IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id);
	}
}