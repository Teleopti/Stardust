using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMeetingRepository : IMeetingRepository
	{
		public void Add(IMeeting root)
		{
			throw new NotImplementedException();
		}

		public void Remove(IMeeting root)
		{
			throw new NotImplementedException();
		}

		public IMeeting Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IMeeting> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IMeeting Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IMeeting> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		IMeeting ILoadAggregateByTypedId<IMeeting, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IMeeting LoadAggregate(Guid id)
		{
			throw new NotImplementedException();
		}

		public ICollection<IMeeting> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			//impl correct when needed
			return new List<IMeeting>();
		}

		public ICollection<IMeeting> Find(DateTimePeriod period, IScenario scenario)
		{
			//change when/if needed
			return new List<IMeeting>();
		}

		public IList<IMeeting> FindMeetingsWithTheseOriginals(ICollection<IMeeting> meetings, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public ICollection<IMeeting> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario, bool includeForOrganizer)
		{
			throw new NotImplementedException();
		}
	}
}