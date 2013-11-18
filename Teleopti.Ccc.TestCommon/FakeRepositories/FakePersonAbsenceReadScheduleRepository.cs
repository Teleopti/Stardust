using System;
using System.Collections.Generic;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAbsenceReadScheduleRepository : IScheduleRepository
	{
		private readonly IPersonAbsence _personAbsence;

		public FakePersonAbsenceReadScheduleRepository(IPersonAbsence personAbsence)
		{
			_personAbsence = personAbsence;
		}

		public void Add(INonversionedPersistableScheduleData entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(INonversionedPersistableScheduleData entity)
		{
			throw new NotImplementedException();
		}

		public INonversionedPersistableScheduleData Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<INonversionedPersistableScheduleData> LoadAll()
		{
			throw new NotImplementedException();
		}

		public INonversionedPersistableScheduleData Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<INonversionedPersistableScheduleData> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public INonversionedPersistableScheduleData Get(Type concreteType, Guid id)
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

		public INonversionedPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			throw new NotImplementedException();
		}
	}
}