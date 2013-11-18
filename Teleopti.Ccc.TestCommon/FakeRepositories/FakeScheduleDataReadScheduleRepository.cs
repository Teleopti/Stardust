using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleDataReadScheduleRepository : IScheduleRepository
	{
		private readonly IScheduleData[] _data;

		public FakeScheduleDataReadScheduleRepository(params IScheduleData[] data)
		{
			_data = data;
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
			var period = _data.First().Period; // max period?
			return ScheduleDictionaryForTest.WithScheduleData(scenario, period, _data);
		}

		public IScheduleDictionary FindSchedulesForPersonOnlyInGivenPeriod(IPerson person,
		                                                                   IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
		                                                                   DateOnlyPeriod period, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public IScheduleDictionary FindSchedulesForPersonsOnlyInGivenPeriod(IEnumerable<IPerson> persons,
		                                                                    IScheduleDictionaryLoadOptions
			                                                                    scheduleDictionaryLoadOptions, DateOnlyPeriod period,
		                                                                    IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public IScheduleRange ScheduleRangeBasedOnAbsence(DateTimePeriod period, IScenario scenario, IPerson person, IAbsence absence)
		{
			throw new NotImplementedException();
		}

		public IScheduleDictionary FindSchedulesForPersons(IScheduleDateTimePeriod period, IScenario scenario,
		                                                   IPersonProvider personsProvider,
		                                                   IScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions,
		                                                   IEnumerable<IPerson> visiblePersons)
		{
			throw new NotImplementedException();
		}

		public INonversionedPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			throw new NotImplementedException();
		}
	}
}