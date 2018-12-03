using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;

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

		public void Clear()
		{
			_absenceAccounts.Clear();
		}

		public IPersonAbsenceAccount Get(Guid id)
		{
			return _absenceAccounts.SingleOrDefault(account => account.Id == id);
		}

		public IEnumerable<IPersonAbsenceAccount> LoadAll()
		{
			return _absenceAccounts;
		}

		public IPersonAbsenceAccount Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IPersonAccountCollection Find(IPerson person, IAbsence absence)
		{
			var personAbsenceAccounts = _absenceAccounts.Where(account => account.Person == person && account.Absence == absence);
			var personAccountCollection = new PersonAccountCollection(person);
			personAbsenceAccounts.ForEach(personAbsenceAccount =>
			{
				if (personAccountCollection.All(pc => pc.Absence != personAbsenceAccount.Absence))
				{
					personAccountCollection.Add(personAbsenceAccount);
				}
			});
			return personAccountCollection;
		}

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
				if (personAccountCollection.All(pc => pc.Absence != personAbsenceAccount.Absence))
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