﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
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
				command.IsReplySuccess = command.TryReplyMessage(personRequest);
			}
		}
		private bool denyRequest(IPersonRequest personRequest, DenyRequestCommand command)
		{
			try
			{
				var denyOption = !command.IsManualDeny ? PersonRequestDenyOption.AutoDeny : PersonRequestDenyOption.None;
				if(command.IsAlreadyAbsent)
					denyOption = denyOption | PersonRequestDenyOption.AlreadyAbsence;

				personRequest.Deny(null, command.DenyReason, _authorization, denyOption);
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
