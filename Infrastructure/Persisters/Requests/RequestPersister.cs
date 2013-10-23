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
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IClearReferredShiftTradeRequests _clearReferredRequests;
		private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;
		private readonly IPrincipalAuthorization _principalAuthorization;

		public RequestPersister(IUnitOfWorkFactory unitOfWorkFactory, 
												IPersonRequestRepository personRequestRepository,
												IClearReferredShiftTradeRequests clearReferredRequests,
												IMessageBrokerIdentifier messageBrokerIdentifier,
												IPrincipalAuthorization principalAuthorization)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personRequestRepository = personRequestRepository;
			_clearReferredRequests = clearReferredRequests;
			_messageBrokerIdentifier = messageBrokerIdentifier;
			_principalAuthorization = principalAuthorization;
		}

		public void Persist(IEnumerable<IPersonRequest> requests)
		{
			if (!_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler))
				return;

			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				foreach (var personRequest in requests)
				{
					if (personRequest.Changed)
					{
						_personRequestRepository.Add(personRequest);
					}
					personRequest.Persisted();
				}
				uow.PersistAll(_messageBrokerIdentifier);
			}
			_clearReferredRequests.ClearReferredShiftTradeRequests();
		}
	}
}