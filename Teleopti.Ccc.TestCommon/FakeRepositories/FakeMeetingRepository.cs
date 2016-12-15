using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeMeetingRepository : IMeetingRepository
	{
		private readonly IList<IMeeting> meetings = new List<IMeeting>();

		public void Add(IMeeting root)
		{
			meetings.Add(root);
		}

		public void Remove(IMeeting root)
		{
			meetings.Remove(root);
		}

		public IMeeting Get(Guid id)
		{
			return meetings.FirstOrDefault(x => x.Id == id);
		}

		public IList<IMeeting> LoadAll()
		{
			return meetings;
		}

		public IMeeting Load(Guid id)
		{
			return meetings.First(x => x.Id == id);
		}

		public long CountAllEntities()
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
			return (from m in meetings
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