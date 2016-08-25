using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAbsenceAccountRepository : IPersonAbsenceAccountRepository
	{

		private readonly IList<IPersonAbsenceAccount> _absenceAccounts = new List<IPersonAbsenceAccount>();

		public void Add(IPersonAbsenceAccount entity)
		{
			_absenceAccounts.Add(entity);
		}

		public void Remove(IPersonAbsenceAccount entity)
		{
			_absenceAccounts.Remove(entity);
		}

		public IPersonAbsenceAccount Get(Guid id)
		{
			return _absenceAccounts.SingleOrDefault(account => account.Id == id);
		}

		public IList<IPersonAbsenceAccount> LoadAll()
		{
			return _absenceAccounts;
		}

		public IPersonAbsenceAccount Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPersonAbsenceAccount> entityCollection)
		{
			entityCollection.ForEach(Add);
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IDictionary<IPerson, IPersonAccountCollection> LoadAllAccounts()
		{
			throw new NotImplementedException();
		}

		public IPersonAccountCollection Find(IPerson person)
		{
			var personAbsenceAccounts = _absenceAccounts.Where(account => account.Person == person);
			var personAccountCollection = new PersonAccountCollection(person);
			personAbsenceAccounts.ForEach(personAbsenceAccount =>
			{
				if (!personAccountCollection.Any(pc => pc.Absence == personAbsenceAccount.Absence))
				{
					personAccountCollection.Add(personAbsenceAccount);
				}
			});
			return personAccountCollection;
		}

		public IDictionary<IPerson, IPersonAccountCollection> FindByUsers(IEnumerable<IPerson> persons)
		{

			var personAccountCollectionDictionary = new Dictionary<IPerson, IPersonAccountCollection>();

			foreach (var person in persons)
			{
				var personAccountCollection = new PersonAccountCollection(person);
				var absenceAccountsForPerson = _absenceAccounts
					.Where(account => account.Person == person && personAccountCollection.All(pac => pac.Absence != account.Absence));

				absenceAccountsForPerson.ForEach(personAccountCollection.Add);
				personAccountCollectionDictionary.Add(person, personAccountCollection);
			}

			return personAccountCollectionDictionary;

		}
	}
}