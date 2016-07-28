using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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
				if (!command.ReplyMessage.IsNullOrEmpty())
				{
					if (tryReplyMessage(personRequest, command))
					{
						command.IsReplySuccess = true;
					}
				}
			}
		}
		private bool tryReplyMessage(IPersonRequest personRequest, DenyRequestCommand command)
		{
			try
			{
				if (!personRequest.CheckReplyTextLength(command.ReplyMessage))
				{
					command.ErrorMessages.Add(UserTexts.Resources.RequestInvalidMessageLength);
					return false;
				}
				personRequest.Reply(command.ReplyMessage);
			}
			catch (InvalidOperationException)
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidMessageModification, personRequest.StatusText));
				return false;
			}
			return true;
		}
		private bool denyRequest(IPersonRequest personRequest, DenyRequestCommand command)
		{
			try
			{
				personRequest.Deny(null, "RequestDenyReasonSupervisor", _authorization, !command.IsManualDeny);
				return true;
			}
			catch (InvalidRequestStateTransitionException)
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidStateTransition, personRequest.StatusText,
					UserTexts.Resources.RequestStatusDenied));
			}

			return false;
		}
	}
}
