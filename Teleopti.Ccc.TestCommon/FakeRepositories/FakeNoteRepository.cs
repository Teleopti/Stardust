using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeNoteRepository : INoteRepository, IEnumerable<INote>
	{
		private readonly IFakeStorage _storage;

		public FakeNoteRepository(IFakeStorage storage)
		{
			_storage = storage;
		}

		public void Add(INote entity)
		{
			entity.SetId(Guid.NewGuid());
			_storage.Add(entity);
		}

		public void Remove(INote entity)
		{
			_storage.Remove(entity);
		}

		public INote Get(Guid id)
		{
			return _storage.Get<INote>(id);
		}

		public IEnumerable<INote> LoadAll()
		{
			return _storage.LoadAll<INote>();
		}

		public INote Load(Guid id)
		{
			throw new NotImplementedException();
		}

		INote ILoadAggregateByTypedId<INote, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public INote LoadAggregate(Guid id)
		{
			return _storage.Get<INote>(id);
		}

		public ICollection<INote> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
		{
			return _storage.LoadAll<INote>().Where(x => x.Scenario == scenario && x.BelongsToPeriod(dateOnlyPeriod) && personCollection.Contains(x.Person)).ToList();
		}

		public IEnumerator<INote> GetEnumerator()
		{
			return _storage.LoadAll<INote>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}