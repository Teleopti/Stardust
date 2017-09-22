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

		public void Resolve(IEnumerable<IPersonAbsenceAccount> conflictingPersonAccounts)
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
				paa.AccountCollection().ForEach(_traceableRefreshService.Refresh);
			});
		}
	}
}