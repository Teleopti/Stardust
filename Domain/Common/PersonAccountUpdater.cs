using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class PersonAccountUpdater : IPersonAccountUpdater
	{
		private readonly IPersonAbsenceAccountRepository _provider;
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

		public bool UpdateForAbsence(IPerson person, IAbsence absence, DateOnly personAbsenceStartDate)
		{
			var personAccounts = _provider.Find(person);
			var accountsForAbsence = personAccounts.Find(absence);

			if (accountsForAbsence == null) return false;

			var accountCollection =
				accountsForAbsence.AccountCollection()
					.Where(personAbsenceAccount => personAbsenceAccount.Period().Contains(personAbsenceStartDate)).ToList();

			if (!accountCollection.Any()) return false;

			accountCollection.ForEach(_traceableRefreshService.Refresh);

			return true;
		}
	}
}
