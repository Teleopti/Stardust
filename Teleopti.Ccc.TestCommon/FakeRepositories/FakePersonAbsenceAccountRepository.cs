using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAbsenceAccountRepository : IPersonAbsenceAccountRepository
	{
		public void Add(IPersonAbsenceAccount entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(IPersonAbsenceAccount entity)
		{
			throw new NotImplementedException();
		}

		public IPersonAbsenceAccount Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonAbsenceAccount> LoadAll()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
			var result = new Dictionary<IPerson, IPersonAccountCollection>();
			var person = PersonFactory.CreatePerson("a");
			result.Add(person, new PersonAccountCollection(person));
			return result;
		}
	}
}