using System;
using System.Collections.Generic;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAssignmentReadScheduleStorage:IScheduleStorage
	{
		private readonly IPersonAssignment _personAssignment;

		public FakePersonAssignmentReadScheduleStorage()
		{
		}

		public FakePersonAssignmentReadScheduleStorage(IPersonAssignment personAssignment)
		{
			_personAssignment = personAssignment;
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
		                                                                    IScheduleDictionaryLoadOptions
			                                                                    scheduleDictionaryLoadOptions,
		                                                                    DateTimePeriod dateTimePeriod, IScenario scenario)
		{
			if (_personAssignment != null)
				return ScheduleDictionaryForTest.WithPersonAssignment(scenario, _personAssignment.Date.Date, _personAssignment);
			return new ScheduleDictionaryForTest(scenario, dateTimePeriod);
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
		                                                                    IScheduleDictionaryLoadOptions
			                                                                    scheduleDictionaryLoadOptions, DateOnlyPeriod period,
		                                                                    IScenario scenario)
		{
			if (_personAssignment != null)
				return ScheduleDictionaryForTest.WithPersonAssignment(scenario, _personAssignment.Date.Date, _personAssignment);
			return new ScheduleDictionaryForTest(scenario, period.StartDate.Date, period.EndDate.Date);
		}

		public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(IEnumerable<IPerson> persons, IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions, DateOnlyPeriod period, IScenario scenario)
		{
			if (_personAssignment != null)
				return ScheduleDictionaryForTest.WithPersonAssignment(scenario, _personAssignment.Date.Date, _personAssignment);
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

		public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			throw new NotImplementedException();
		}
	}
}