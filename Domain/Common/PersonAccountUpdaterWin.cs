using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	/// <summary>
	/// Person account updater that works with the Win project's FilteredPeopleHolder's domain data
	/// </summary>
	/// <remarks>
	/// Used for the win fat client where all the data is already loaded in the FilteredPeopleHolder
	/// </remarks>
	public class PersonAccountUpdaterWin : IPersonAccountUpdater
	{
		private readonly IPeopleAccountUpdaterWinProvider _provider;

		public PersonAccountUpdaterWin(IPeopleAccountUpdaterWinProvider provider)
		{
			_provider = provider;
		}

		public void Update(IPerson person)
		{
			refreshAllAccounts(person);
		}

		private void refreshAllAccounts(IPerson person)
		{
			var personAccounts = _provider.GetPersonAccounts(person);
			foreach (var personAccount in personAccounts)
			{
				foreach (var account in personAccount.AccountCollection())
				{
					_provider.GetRefreshService().Refresh(account, _provider.GetUnitOfWork);
				}
			}
		}
	}
}