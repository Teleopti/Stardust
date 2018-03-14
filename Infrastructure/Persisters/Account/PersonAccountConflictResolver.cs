using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Tracking;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public class PersonAccountConflictResolver : IPersonAccountConflictResolver
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ITraceableRefreshService _traceableRefreshService;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;

		public PersonAccountConflictResolver(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
																					ITraceableRefreshService traceableRefreshService, 
																					IPersonAbsenceAccountRepository personAbsenceAccountRepository)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_traceableRefreshService = traceableRefreshService;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
		}

		public void Resolve(IEnumerable<IPersonAbsenceAccount> conflictingPersonAccounts, IScheduleDictionary scheduleDictionary)
		{
			var uow = _currentUnitOfWorkFactory.Current().CurrentUnitOfWork();
			conflictingPersonAccounts.ForEach(paa =>
			{
				var foundAccount = _personAbsenceAccountRepository.Get(paa.Id.GetValueOrDefault());
				var removedAccounts = paa.AccountCollection().Except(foundAccount.AccountCollection()).ToList();
				foreach (var removedAccount in removedAccounts)
				{
						paa.Remove(removedAccount);
				}
				uow.Remove(foundAccount);
				uow.Refresh(paa);

				foreach (var account in paa.AccountCollection())
				{
					_traceableRefreshService.Refresh(account);
					if(scheduleDictionary == null) continue;
					var period = scheduleDictionary.Period.LoadedPeriod().ToDateOnlyPeriod(TimeZoneInfo.Utc);
					var scheduleDays = scheduleDictionary[paa.Person].ScheduledDayCollection(period).ToList();
					var loaded = account.Owner.Absence.Tracker.TrackForReset(account.Owner.Absence, scheduleDays);
					account.Track(loaded);
				}
				//paa.AccountCollection().ForEach(_traceableRefreshService.Refresh);
			});
		}
	}
}