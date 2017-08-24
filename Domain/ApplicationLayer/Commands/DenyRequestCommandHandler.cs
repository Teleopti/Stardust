using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class DenyRequestCommandHandler : IHandleCommand<DenyRequestCommand>
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRequestCheckAuthorization _authorization;

		public DenyRequestCommandHandler(IPersonRequestRepository personRequestRepository,
			IPersonRequestCheckAuthorization authorization)
		{
			_personRequestRepository = personRequestRepository;
			_authorization = authorization;
		}

		public void Handle(DenyRequestCommand command)
		{
			command.ErrorMessages = new List<string>();

			var personRequest = _personRequestRepository.Get(command.PersonRequestId);

			if (personRequest != null && denyRequest(personRequest, command))
			{
				command.AffectedRequestId = command.PersonRequestId;
				command.IsReplySuccess = command.TryReplyMessage(personRequest);
			}
		}
		private bool denyRequest(IPersonRequest personRequest, DenyRequestCommand command)
		{
			if (personRequest.IsDenied)
			{
				return invalidRequestState(personRequest, command);
			}

			try
			{
				var denyOption = !command.IsManualDeny ? PersonRequestDenyOption.AutoDeny : PersonRequestDenyOption.None;
				denyOption = denyOption | command.DenyOption.GetValueOrDefault(PersonRequestDenyOption.None);

				personRequest.Deny(command.DenyReason, _authorization, null, denyOption);
				return true;
			}
			catch (InvalidRequestStateTransitionException)
			{
				invalidRequestState(personRequest, command);
			}

			return false;
		}

		private static bool invalidRequestState(IPersonRequest personRequest, IRequestCommand command)
		{
			if (personRequest.IsDeleted)
			{
				command.ErrorMessages.Add(UserTexts.Resources.RequestHasBeenDeleted);
			}
			else
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidStateTransition, personRequest.StatusText,
					UserTexts.Resources.RequestStatusDenied));
			}

			return false;
		}
	}
}
