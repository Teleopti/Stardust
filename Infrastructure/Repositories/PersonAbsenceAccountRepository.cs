using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonAbsenceAccountRepository : Repository<IPersonAbsenceAccount>,
													IPersonAbsenceAccountRepository
	{
		public static PersonAbsenceAccountRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new PersonAbsenceAccountRepository(currentUnitOfWork, null, null);
		}

		public static PersonAbsenceAccountRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new PersonAbsenceAccountRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public PersonAbsenceAccountRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		public IDictionary<IPerson, IPersonAccountCollection> LoadAllAccounts()
		{
			var result = Session.CreateCriteria(typeof (PersonAbsenceAccount))
						.Fetch("accountCollection")
						.Fetch("Person")
						.Fetch("Absence")
						.SetResultTransformer(Transformers.DistinctRootEntity)
						.List<IPersonAbsenceAccount>();
			return new dic(result.GroupBy(k => k.Person).ToDictionary(k => k.Key, v =>
			{
				IPersonAccountCollection collection = new PersonAccountCollection(v.Key);
				v.ForEach(collection.Add);
				return collection;
			}));
		}

		public IDictionary<IPerson, IPersonAccountCollection> FindByUsers(IEnumerable<IPerson> persons)
		{
			var result = new List<IPersonAbsenceAccount>();
			foreach (var personBatch in persons.Batch(400))
			{
				result.AddRange(Session.CreateCriteria(typeof(PersonAbsenceAccount))
					   .Fetch("accountCollection")
					   .Fetch("Person")
					   .Fetch("Absence")
					   .Add(Restrictions.InG("Person", personBatch.ToArray()))
					   .SetResultTransformer(Transformers.DistinctRootEntity)
					   .List<IPersonAbsenceAccount>());    
			}

			return new dic(result.GroupBy(k => k.Person).ToDictionary(k => k.Key, v =>
			{
				IPersonAccountCollection collection = new PersonAccountCollection(v.Key);
				v.ForEach(collection.Add);
				return collection;
			}));
		}

		public IDictionary<IPerson, IPersonAccountCollection> LoadByUsers(IEnumerable<IPerson> persons)
		{
			var result = new List<IPersonAbsenceAccount>();
			foreach (var personBatch in persons.Batch(400))
			{
				result.AddRange(Session.CreateCriteria(typeof(PersonAbsenceAccount))
					.Fetch("accountCollection")
					.Add(Restrictions.InG("Person", personBatch.ToArray()))
					.SetResultTransformer(Transformers.DistinctRootEntity)
					.List<IPersonAbsenceAccount>());
			}
			
			return new dic(result.GroupBy(k => k.Person).ToDictionary(k => k.Key, v =>
			{
				IPersonAccountCollection collection = new PersonAccountCollection(v.Key);
				v.ForEach(collection.Add);
				return collection;
			}));
		}

		public IPersonAccountCollection Find(IPerson person)
		{
			var result = Session.CreateCriteria(typeof(PersonAbsenceAccount))
						.Fetch("accountCollection")
						.Add(Restrictions.Eq("Person",person))
						.SetResultTransformer(Transformers.DistinctRootEntity)
						.List<IPersonAbsenceAccount>();
			var personAccountCollection = new PersonAccountCollection(person);
			foreach (var personAbsenceAccount in result)
			{
				personAccountCollection.Add(personAbsenceAccount);
			}
			return personAccountCollection;
		}

		public IPersonAccountCollection Find(IPerson person, IAbsence absence)
		{
			var result = Session.CreateCriteria(typeof(PersonAbsenceAccount))
						.Fetch("accountCollection")
						.Add(Restrictions.Eq("Person",person))
						.Add(Restrictions.Eq("Absence",absence))
						.SetResultTransformer(Transformers.DistinctRootEntity)
						.List<IPersonAbsenceAccount>();
			var personAccountCollection = new PersonAccountCollection(person);
			foreach (var personAbsenceAccount in result)
			{
				personAccountCollection.Add(personAbsenceAccount);
			}
			return personAccountCollection;
		}

		private class dic : IDictionary<IPerson, IPersonAccountCollection>
		{
			private readonly IDictionary<IPerson, IPersonAccountCollection> _dictionary;

			public dic(IDictionary<IPerson, IPersonAccountCollection> dictionary)
			{
				_dictionary = dictionary;
			}

			public IEnumerator<KeyValuePair<IPerson, IPersonAccountCollection>> GetEnumerator()
			{
				return _dictionary.GetEnumerator();
			}

			public void Add(KeyValuePair<IPerson, IPersonAccountCollection> item)
			{
				_dictionary.Add(item);
			}

			public void Clear()
			{
				_dictionary.Clear();
			}

			public bool Contains(KeyValuePair<IPerson, IPersonAccountCollection> item)
			{
				return _dictionary.Contains(item);
			}

			public void CopyTo(KeyValuePair<IPerson, IPersonAccountCollection>[] array, int arrayIndex)
			{
				_dictionary.CopyTo(array, arrayIndex);
			}

			public bool Remove(KeyValuePair<IPerson, IPersonAccountCollection> item)
			{
				return _dictionary.Remove(item);
			}

			public int Count => _dictionary.Count;

			public bool IsReadOnly => _dictionary.IsReadOnly;

			public bool ContainsKey(IPerson key)
			{
				return _dictionary.ContainsKey(key);
			}

			public void Add(IPerson key, IPersonAccountCollection value)
			{
				_dictionary.Add(key, value);
			}

			public bool Remove(IPerson key)
			{
				return _dictionary.Remove(key);
			}

			public bool TryGetValue(IPerson key, out IPersonAccountCollection value)
			{
				return _dictionary.TryGetValue(key, out value);
			}

			public IPersonAccountCollection this[IPerson key]
			{
				get
				{
					if (_dictionary.TryGetValue(key, out var collection))
					{
						return collection;
					}
					collection = new PersonAccountCollection(key);
					_dictionary.Add(key, collection);
					return collection;
				}
				set { _dictionary[key] = value; }
			}

			public ICollection<IPerson> Keys => _dictionary.Keys;

			public ICollection<IPersonAccountCollection> Values => _dictionary.Values;

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}