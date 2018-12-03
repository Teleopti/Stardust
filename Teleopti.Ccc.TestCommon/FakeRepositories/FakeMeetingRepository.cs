using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMeetingRepository : IMeetingRepository
	{
		private readonly IFakeStorage _storage;

		public FakeMeetingRepository(IFakeStorage storage)
		{
			_storage = storage;
		}

		public void Add(IMeeting root)
		{
			_storage.Add(root);
		}

		public void Remove(IMeeting root)
		{
			_storage.Remove(root);
		}

		public IMeeting Get(Guid id)
		{
			return _storage.Get<IMeeting>(id);
		}

		public IEnumerable<IMeeting> LoadAll()
		{
			return _storage.LoadAll<IMeeting>().ToList();
		}

		public IMeeting Load(Guid id)
		{
			return _storage.LoadAll<IMeeting>().First(x => x.Id == id);
		}

		IMeeting ILoadAggregateByTypedId<IMeeting, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public IMeeting LoadAggregate(Guid id)
		{
			throw new NotImplementedException();
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

		public ICollection<IMeeting> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario, bool includeForOrganizer = true)
		{
			return (from m in _storage.LoadAll<IMeeting>()
					where m.Scenario == scenario
				let matchPersons = includeForOrganizer ?
					m.MeetingPersons.Select(mp => mp.Person).Append(m.Organizer) :
					m.MeetingPersons.Select(mp => mp.Person)
				where matchPersons.Any(persons.Contains)
				where new DateOnlyPeriod(m.StartDate, m.EndDate).Intersection(period).HasValue
				select m
			).ToList();
		}

		public void Has(IMeeting meeting)
		{
			Add(meeting);
		}
	}
}