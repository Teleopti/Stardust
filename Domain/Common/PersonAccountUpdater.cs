using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	/// <summary>
	/// Person account updater that works with the Win project's FilteredPeopleHolder's domain data
	/// </summary>
	/// <remarks>
	/// Used for the win fat client where all the data is already loaded in the FilteredPeopleHolder
	/// </remarks>
	public class PersonAccountUpdater : IPersonAccountUpdater
	{
		private readonly IPersonAbsenceAccountRepository  _provider;
	    private readonly ITraceableRefreshService _traceableRefreshService;

	    public PersonAccountUpdater(IPersonAbsenceAccountRepository provider, ITraceableRefreshService traceableRefreshService)
		{
		    _provider = provider;
		    _traceableRefreshService = traceableRefreshService;
		}

        public void Update(IPerson person)
		{
			var personAccounts = _provider.Find(person);
			
			foreach (var personAccount in personAccounts)
			{
				foreach (var account in personAccount.AccountCollection())
				{
					_traceableRefreshService.Refresh(account);
				}
			}
		}
	}
}