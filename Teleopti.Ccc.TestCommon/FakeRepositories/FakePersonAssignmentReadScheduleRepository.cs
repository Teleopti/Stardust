using System;
using System.Collections.Generic;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAssignmentReadScheduleRepository:IScheduleRepository
	{
		private readonly IPersonAssignment _personAssignment;

		public FakePersonAssignmentReadScheduleRepository()
		{
		}

		public FakePersonAssignmentReadScheduleRepository(IPersonAssignment personAssignment)
		{
			_personAssignment = personAssignment;
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
		                                                                    IScheduleDictionaryLoadOptions
			                                                                    scheduleDictionaryLoadOptions,
		                                                                    DateTimePeriod dateTimePeriod, IScenario scenario)
		{
			if (_personAssignment != null)
				return ScheduleDictionaryForTest.WithPersonAssignment(scenario, _personAssignment.Date, _personAssignment);
			return new ScheduleDictionaryForTest(scenario, dateTimePeriod);
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
		                                                                    IScheduleDictionaryLoadOptions
			                                                                    scheduleDictionaryLoadOptions, DateOnlyPeriod period,
		                                                                    IScenario scenario)
		{
			if (_personAssignment != null)
				return ScheduleDictionaryForTest.WithPersonAssignment(scenario, _personAssignment.Date, _personAssignment);
			return new ScheduleDictionaryForTest(scenario, period.StartDate.Date, period.EndDate.Date);
		}

		public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(IEnumerable<IPerson> persons, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateOnlyPeriod period, IScenario scenario)
		{
			if (_personAssignment != null)
				return ScheduleDictionaryForTest.WithPersonAssignment(scenario, _personAssignment.Date, _personAssignment);
			return new ScheduleDictionaryForTest(scenario, period.StartDate.Date, period.EndDate.Date);
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