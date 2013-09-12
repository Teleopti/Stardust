using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	/// <summary>
	/// Person account updater that works with the Win project's FilteredPeopleHolder's domain data
	/// </summary>
	/// <remarks>
	/// Used for the win fat client where all the data is already loaded in the FilteredPeopleHolder
	/// </remarks>
	public class PersonAccountUpdaterForSdk : IPersonAccountUpdater
	{
		private readonly IPeopleAccountUpdaterInteraction _interaction;

		public PersonAccountUpdaterForSdk(IPeopleAccountUpdaterInteraction interaction)
		{
			_interaction = interaction;
		}

		public void UpdateOnTermination(DateOnly terminalDate, IPerson person)
		{
			refreshAllAccounts(person);
		}

		public void UpdateOnActivation(IPerson person)
		{
			refreshAllAccounts(person);
		}

		private void refreshAllAccounts(IPerson person)
		{
			var personAccounts = _interaction.PersonAccounts(person);
			foreach (var personAccount in personAccounts)
			{
				foreach (var account in personAccount.AccountCollection())
				{
					_interaction.RefreshService.Refresh(account, _interaction.UnitOfWork);
				}
			}
		}
	}
}