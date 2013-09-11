using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	/// <summary>
	/// Person account updater that works with the Win project's FilteredPeopleHolder's domain data
	/// </summary>
	/// <remarks>
	/// Used for the win fat client where all the data is already loaded in the FilteredPeopleHolder
	/// </remarks>
	public class PersonAccountUpdaterForWin : IPersonAccountUpdater
	{
		private readonly IPeopleAccountUpdaterProvider _provider;

		public PersonAccountUpdaterForWin(IPeopleAccountUpdaterProvider provider)
		{
			_provider = provider;
		}

		public void UpdateOnTermination(DateOnly terminalDate, IPerson person)
		{
			refreshAllAccounts(person);
		}

		public void UpdateOnActivation(IPerson person)
		{
			refreshAllAccounts(person);
		}

		//checkIfAuthorized(foundPerson, dateFrom);

		//var accounts = _personAbsenceAccountRepository.Find(foundPerson);
		//var personAccount = accounts.Find(foundAbsence, dateFrom);
		//if (personAccount == null || !personAccount.StartDate.Equals(dateFrom))
		//{
		//	personAccount = createPersonAccount(foundAbsence, accounts, dateFrom);
		//}

		//setPersonAccount(personAccount, command);

		//unitOfWork.PersistAll();

		private void refreshAllAccounts(IPerson person)
		{
			var personAccounts = _provider.PersonAccounts(person);
			foreach (var personAccount in personAccounts)
			{
				foreach (var temp in personAccount.Value)
				{
					foreach (var account in temp.AccountCollection())
					{
						_provider.RefreshService.Refresh(account, _provider.UnitOfWork);
					}
				}
			}
		}

	
	}
}