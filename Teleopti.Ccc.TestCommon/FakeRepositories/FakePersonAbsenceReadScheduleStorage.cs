using System;
using System.Collections.Generic;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAbsenceReadScheduleStorage : IScheduleStorage
	{
		private readonly IPersonAbsence _personAbsence;

		public FakePersonAbsenceReadScheduleStorage(IPersonAbsence personAbsence)
		{
			_personAbsence = personAbsence;
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
			throw new NotImplementedException();
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
			return ScheduleDictionaryForTest.WithPersonAbsence(scenario, _personAbsence.Period, _personAbsence);
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
		                                                          IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
		                                                          DateOnlyPeriod period, IScenario scenario)
		{
			return ScheduleDictionaryForTest.WithPersonAbsence(scenario, _personAbsence.Period, _personAbsence);
		}

		public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(IEnumerable<IPerson> persons, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateOnlyPeriod period, IScenario scenario)
		{
			return ScheduleDictionaryForTest.WithPersonAbsence(scenario, _personAbsence.Period, _personAbsence);
		}

		public IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence)
		{
			throw new NotImplementedException();
		}

		public IScheduleDictionary FindSchedulesForPersons(IScheduleDateTimePeriod period, IScenario scenario, IPersonProvider personsProvider, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, IEnumerable<IPerson> visiblePersons)
		{
			throw new NotImplementedException();
		}

		public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			throw new NotImplementedException();
		}
	}
}