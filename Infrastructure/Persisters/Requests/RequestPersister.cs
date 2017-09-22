using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Persisters.Requests
{
	public class RequestPersister : IRequestPersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IClearReferredShiftTradeRequests _clearReferredRequests;
		private readonly IInitiatorIdentifier _initiatorIdentifier;
		private readonly ICurrentAuthorization _authorization;

		public RequestPersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
												IPersonRequestRepository personRequestRepository,
												IClearReferredShiftTradeRequests clearReferredRequests,
												IInitiatorIdentifier initiatorIdentifier,
												ICurrentAuthorization authorization)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_personRequestRepository = personRequestRepository;
			_clearReferredRequests = clearReferredRequests;
			_initiatorIdentifier = initiatorIdentifier;
			_authorization = authorization;
		}

		public void Persist(IEnumerable<IPersonRequest> requests)
		{
			if (!_authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
				return;

			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
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