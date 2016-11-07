using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeNoteRepository : INoteRepository, IEnumerable<INote>
	{
		private readonly IList<INote> _notes = new List<INote>();

		public FakeNoteRepository(params INote[] notes)
		{
			notes.ForEach(Add);
		}

		public void Add(INote entity)
		{
			entity.SetId(Guid.NewGuid());
			_notes.Add(entity);
		}

		public void Remove(INote entity)
		{
			_notes.Remove(entity);
		}

		public INote Get(Guid id)
		{
			return _notes.FirstOrDefault(note => note.Id == id);
		}

		public IList<INote> LoadAll()
		{
			return _notes;
		}

		public INote Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		INote ILoadAggregateByTypedId<INote, Guid>.LoadAggregate(Guid id)
		{
			return LoadAggregate(id);
		}

		public INote LoadAggregate(Guid id)
		{
			return _notes.First(x => x.Id == id);
		}

		public IList<INote> Find(DateTimePeriod period, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public ICollection<INote> Find(DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
		{
			return _notes.Where(x => x.Scenario == scenario && x.BelongsToPeriod(dateOnlyPeriod) && personCollection.Contains(x.Person)).ToList();
		}

		public INote Find(DateOnly dateOnly, IPerson person, IScenario scenario)
		{
			return _notes.SingleOrDefault(note => note.NoteDate == dateOnly && note.Person == person && note.Scenario == scenario);
		}

		public IEnumerator<INote> GetEnumerator()
		{
			return _notes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}