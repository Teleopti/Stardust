using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public class PersonAccountConflictResolver : IPersonAccountConflictResolver
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ITraceableRefreshService _traceableRefreshService;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;

		public PersonAccountConflictResolver(IUnitOfWorkFactory unitOfWorkFactory,
																					ITraceableRefreshService traceableRefreshService, 
																					IPersonAbsenceAccountRepository personAbsenceAccountRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_traceableRefreshService = traceableRefreshService;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
		}

		public void Resolve(IEnumerable<IPersonAbsenceAccount> conflictingPersonAccounts)
		{
			var uow = _unitOfWorkFactory.CurrentUnitOfWork();
			//rk - don't understand anything here... just copy/paste from before and tried to include useful tests...
			conflictingPersonAccounts.ForEach(paa =>
			{
				//var foundAccount = _personAbsenceAccountRepository.Get(paa.Id.GetValueOrDefault());
				//if (foundAccount != null)
				//{
				//	var removedAccounts = paa.AccountCollection().Except(foundAccount.AccountCollection()).ToList();
				//	foreach (IAccount removedAccount in removedAccounts)
				//	{
				//		paa.Remove(removedAccount);
				//	}
				//	unitOfWork.Remove(foundAccount);
				//	unitOfWork.Refresh(paa);
				//}
				//var foundAccount = _personAbsenceAccountRepository.Get(paa.Id.Value);
				uow.Refresh(paa);
				paa.AccountCollection().ForEach(_traceableRefreshService.Refresh);
				//_personAbsenceAccountValidator.Validate(paa);
			});
		}
	}
}