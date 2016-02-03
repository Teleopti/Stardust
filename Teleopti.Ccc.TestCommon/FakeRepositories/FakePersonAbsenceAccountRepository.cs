using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon.FakeData;
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
			throw new NotImplementedException();
		}

		public IDictionary<IPerson, IPersonAccountCollection> FindByUsers(IEnumerable<IPerson> persons)
		{

			var personAccountCollectionDictionary = new Dictionary<IPerson, IPersonAccountCollection>();

			foreach (var person in persons)
			{
				var personAccountCollection = new PersonAccountCollection (person);
				var absenceAccountsForPerson = _absenceAccounts.Where (account => account.Person == person);
				absenceAccountsForPerson.ForEach(personAccountCollection.Add);
				personAccountCollectionDictionary.Add(person, personAccountCollection);
			}

			return personAccountCollectionDictionary;
			
		}
	}
}