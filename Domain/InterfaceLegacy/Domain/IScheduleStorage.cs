using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IScheduleStorage
	{
		void Add(IPersistableScheduleData scheduleData);
		void Remove(IPersistableScheduleData scheduleData);
		IPersistableScheduleData Get(Type concreteType, Guid id);

		IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(
			IPerson person,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateTimePeriod dateTimePeriod,
			IScenario scenario);

		IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(
			IPerson person,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateOnlyPeriod period,
			IScenario scenario);

		IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(
			IEnumerable<IPerson> persons,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			DateOnlyPeriod period,
			IScenario scenario);

		IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence = null);

		IScheduleDictionary FindSchedulesForPersons(
			IScheduleDateTimePeriod period,
			IScenario scenario,
			IPersonProvider personsProvider,
			ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
			IEnumerable<IPerson> visiblePersons);

		IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id);
	}
}