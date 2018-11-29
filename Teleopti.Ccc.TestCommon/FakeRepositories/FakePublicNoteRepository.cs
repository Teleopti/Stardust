using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePublicNoteRepository : IPublicNoteRepository, IEnumerable<IPublicNote>
	{
		private readonly IFakeStorage _storage;
		
		public FakePublicNoteRepository(IFakeStorage storage)
		{
			_storage = storage;
		}

		public void Add (IPublicNote entity)
		{
			entity.SetId (Guid.NewGuid());
			_storage.Add (entity);
		}

		public void Remove (IPublicNote entity)
		{
			_storage.Remove (entity);
		}

		public IPublicNote Get (Guid id)
		{
			return _storage.Get<IPublicNote>(id);
		}

		public IEnumerable<IPublicNote> LoadAll()
		{
			return _storage.LoadAll<IPublicNote>();
		}

		public IPublicNote Load (Guid id)
		{
			throw new NotImplementedException();
		}

		IPublicNote ILoadAggregateByTypedId<IPublicNote, Guid>.LoadAggregate (Guid id)
		{
			return LoadAggregate (id);
		}

		public IPublicNote LoadAggregate (Guid id)
		{
			return _storage.Get<IPublicNote>(id);
		}

		public IList<IPublicNote> Find (DateTimePeriod period, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPublicNote> Find (DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
		{
			return _storage.LoadAll<IPublicNote>().Where(x => x.Scenario == scenario && x.BelongsToPeriod(dateOnlyPeriod) && personCollection.Contains(x.Person)).ToList();
		}

		public IPublicNote Find (DateOnly dateOnly, IPerson person, IScenario scenario)
		{
			return _storage.LoadAll<IPublicNote>().SingleOrDefault (note => note.NoteDate == dateOnly && note.Person == person && note.Scenario == scenario);
		}

		public IEnumerator<IPublicNote> GetEnumerator()
		{
			return _storage.LoadAll<IPublicNote>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}