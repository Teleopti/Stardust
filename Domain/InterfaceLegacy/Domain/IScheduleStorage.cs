using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IScheduleStorage :IFindSchedulesForPersons
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
			IScenario scenario,
			bool dontTrackChangesForPersonAssignment = false);
		

		IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence);

		IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id);
	}
}