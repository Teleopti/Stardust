using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Requests
{
	public class RequestPersister : IRequestPersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IClearReferredShiftTradeRequests _clearReferredRequests;
		private readonly IInitiatorIdentifier _initiatorIdentifier;
		private readonly IPrincipalAuthorization _principalAuthorization;

		public RequestPersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
												IPersonRequestRepository personRequestRepository,
												IClearReferredShiftTradeRequests clearReferredRequests,
												IInitiatorIdentifier initiatorIdentifier,
												IPrincipalAuthorization principalAuthorization)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_personRequestRepository = personRequestRepository;
			_clearReferredRequests = clearReferredRequests;
			_initiatorIdentifier = initiatorIdentifier;
			_principalAuthorization = principalAuthorization;
		}

		public void Persist(IEnumerable<IPersonRequest> requests)
		{
			if (!_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
				return;

			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				foreach (var personRequest in requests)
				{
					if (personRequest.Changed)
					{
						_personRequestRepository.Add(personRequest);
					}
					personRequest.Persisted();
				}
				uow.PersistAll(_initiatorIdentifier);
			}
			_clearReferredRequests.ClearReferredShiftTradeRequests();
		}
	}
}