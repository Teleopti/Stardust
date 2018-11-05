using System;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonAbsenceAccountRepository : Repository<IPersonAbsenceAccount>,
													IPersonAbsenceAccountRepository
	{
		public PersonAbsenceAccountRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public PersonAbsenceAccountRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public IDictionary<IPerson, IPersonAccountCollection> LoadAllAccounts()
		{
			var result = Session.CreateCriteria(typeof (PersonAbsenceAccount))
						.SetFetchMode("accountCollection", FetchMode.Join)
						.SetFetchMode("Person",FetchMode.Join)
						.SetFetchMode("Absence",FetchMode.Join)
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
					   .SetFetchMode("accountCollection", FetchMode.Join)
					   .SetFetchMode("Person", FetchMode.Join)
					   .SetFetchMode("Absence", FetchMode.Join)
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
					.SetFetchMode("accountCollection", FetchMode.Join)
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
						.SetFetchMode("accountCollection", FetchMode.Join)
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

		private class dic : AbstractDictionary<IPerson, IPersonAccountCollection>
		{
			public dic(IDictionary<IPerson, IPersonAccountCollection> dictionary) : base(dictionary)
			{
			}

			public override IPersonAccountCollection this[IPerson key]
			{
				get {
					if (base.TryGetValue(key, out var collection))
					{
						return collection;
					}
					collection = new PersonAccountCollection(key);
					base.Add(key,collection);
					return collection;
				}
			}
		}
	}
}