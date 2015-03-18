using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePublicNoteRepository : IPublicNoteRepository, IEnumerable<IPublicNote>
	{

		private readonly IList<IPublicNote> _notes = new List<IPublicNote>();

		
		public FakePublicNoteRepository(IPublicNote note)
		{
			Has(note);
		}

		public void Has(IPublicNote note)
		{
			_notes.Add(note);
		}


		public void Add (IPublicNote entity)
		{
			_notes.Add (entity);
		}

		public void Remove (IPublicNote entity)
		{
			_notes.Remove (entity);
		}

		public IPublicNote Get (Guid id)
		{
			return _notes.FirstOrDefault();
		}

		public IList<IPublicNote> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPublicNote Load (Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange (IEnumerable<IPublicNote> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		IPublicNote ILoadAggregateByTypedId<IPublicNote, Guid>.LoadAggregate (Guid id)
		{
			return LoadAggregate (id);
		}

		public IPublicNote LoadAggregate (Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPublicNote> Find (DateTimePeriod period, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPublicNote> Find (DateOnlyPeriod dateOnlyPeriod, IEnumerable<IPerson> personCollection, IScenario scenario)
		{
			return new List<IPublicNote>() {Get (Guid.Empty)};
		}

		public IPublicNote Find (DateOnly dateOnly, IPerson person, IScenario scenario)
		{
			return Get(Guid.Empty);
		}

		public IEnumerator<IPublicNote> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}