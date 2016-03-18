using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class DenyRequestCommandHandler : IHandleCommand<DenyRequestCommand>
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRequestCheckAuthorization _authorization;

		public DenyRequestCommandHandler(IPersonRequestRepository personRequestRepository, IPersonRequestCheckAuthorization authorization)
		{
			_personRequestRepository = personRequestRepository;
			_authorization = authorization;
		}

		public void Handle(DenyRequestCommand command)
		{
			var personRequest = _personRequestRepository.Get(command.PersonRequestId);
			if (personRequest != null && denyRequest(personRequest, !command.IsManualDeny))
			{
				command.AffectedRequestId = command.PersonRequestId;
			}			
		}

		private bool denyRequest(IPersonRequest personRequest, bool isAutoDeny)
		{
			try
			{
				personRequest.Deny(null, "RequestDenyReasonSupervisor", _authorization, isAutoDeny);
				return true;
			}
			catch (InvalidRequestStateTransitionException)
			{
				return false;
			}
			
		}
	}
}
