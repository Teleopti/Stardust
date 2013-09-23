using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers
{
	public class FilteredPeopleAccountUpdater : IPersonAccountUpdater
	{
		private readonly FilteredPeopleHolder _filteredPeopleHolder;
		private readonly ITraceableRefreshService _traceableRefreshService;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public FilteredPeopleAccountUpdater(FilteredPeopleHolder filteredPeopleHolder, ITraceableRefreshService traceableRefreshService, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_filteredPeopleHolder = filteredPeopleHolder;
			_traceableRefreshService = traceableRefreshService;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Update(IPerson person)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var accounts = _filteredPeopleHolder.GetPersonAccounts(person);
				foreach (var personAbsenceAccount in accounts)
				{
					foreach (var account in personAbsenceAccount.AccountCollection())
					{
						_traceableRefreshService.Refresh(account);
					}
				}
			}
		}
	}
}