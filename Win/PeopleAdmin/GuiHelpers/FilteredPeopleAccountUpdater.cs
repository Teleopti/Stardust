using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers
{
	/// <summary>
	/// Person account updater that works with the Win project's FilteredPeopleHolder's domain data
	/// </summary>
	/// <remarks>
	/// Used for the win fat client where all the data is already loaded in the FilteredPeopleHolder
	/// </remarks>
	public class FilteredPeopleAccountUpdater : IPersonAccountUpdater
	{
		private readonly FilteredPeopleHolder _filteredPeopleHolder;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public FilteredPeopleAccountUpdater (FilteredPeopleHolder filteredPeopleHolder, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_filteredPeopleHolder = filteredPeopleHolder;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Update (IPerson person)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var accounts = _filteredPeopleHolder.GetPersonAccounts (person);
				foreach (var personAbsenceAccount in accounts)
				{
					foreach (var account in personAbsenceAccount.AccountCollection())
					{
						_filteredPeopleHolder.RefreshService.Refresh (account);
					}
				}
			}
		}

		public bool UpdateForAbsence (IPerson person, IAbsence absence, DateOnly personAbsenceStartDate)
		{
			throw new System.NotImplementedException();
		}

	}
}