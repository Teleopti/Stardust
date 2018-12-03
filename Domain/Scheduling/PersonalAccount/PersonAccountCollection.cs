using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.PersonalAccount
{
	public class PersonAccountCollection : IPersonAccountCollection
	{
		private readonly IList<IPersonAbsenceAccount> _personAbsenceAccountCollection;

		public PersonAccountCollection(IPerson person)
		{
			Person = person;
			_personAbsenceAccountCollection = new List<IPersonAbsenceAccount>();
		}

		public void Add(IPersonAbsenceAccount personAbsenceAccount)
		{
			checkIfValidToAdd(personAbsenceAccount);
			_personAbsenceAccountCollection.Add(personAbsenceAccount);
		}

		public ReadOnlyCollection<IPersonAbsenceAccount> PersonAbsenceAccounts()
		{
			return new ReadOnlyCollection<IPersonAbsenceAccount>(_personAbsenceAccountCollection);
		}

		public void Add(IAbsence absence, IAccount account)
		{
			var personAbsenceAccount = Find(absence);
			if (personAbsenceAccount != null)
			{
				personAbsenceAccount.Add(account);                
			}
			else
			{
				createAndAdd(absence, account);
			}
		}

		public void Remove(IAccount account)
		{
			foreach (var paAcc in _personAbsenceAccountCollection)
			{
				paAcc.Remove(account);
			}
		}

		private void createAndAdd(IAbsence absence, IAccount account)
		{
			var createdAccount = new PersonAbsenceAccount(Person, absence);
			createdAccount.Add(account);
			Add(createdAccount);
		}

		private void checkIfValidToAdd(IPersonAbsenceAccount personAbsenceAccount)
		{
			if(!personAbsenceAccount.Person.Equals(Person))
				throw new ArgumentException("AccountForPersonCollection only allows " + personAbsenceAccount.Person);
			if(collectionContains(personAbsenceAccount.Absence))
				throw new ArgumentException("AccountForPersonCollection already contains absence " + personAbsenceAccount.Absence);
		}

		public IPerson Person { get; private set; }

		public IEnumerable<IAccount> AllPersonAccounts()
		{
			foreach (var paAcc in _personAbsenceAccountCollection)
			{
				foreach (var account in paAcc.AccountCollection())
				{
					yield return account;
				}
			}
		}

		private bool collectionContains(IAbsence absence)
		{
			foreach (var acc in _personAbsenceAccountCollection)
			{
				if(acc.Absence.Equals(absence))
					return true;
			}
			return false;
		}

		public IPersonAbsenceAccount Find(IAbsence absence)
		{
			foreach (var personAbsenceAccount in _personAbsenceAccountCollection)
			{
				if (personAbsenceAccount.Absence.Equals(absence))
					return personAbsenceAccount;
			}
			return null;
		}

		public IAccount Find(IAbsence absence, DateOnly dateOnly)
		{
			var accounts = Find(absence);
			return accounts?.Find(dateOnly);
		}

		public IEnumerable<IAccount> Find(DateOnly dateOnly)
		{
			foreach (var paAcc in _personAbsenceAccountCollection)
			{
				var account = paAcc.Find(dateOnly);
				if(account != null)
					yield return account;
			}
		}

		public IEnumerator<IPersonAbsenceAccount> GetEnumerator()
		{
			return _personAbsenceAccountCollection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}