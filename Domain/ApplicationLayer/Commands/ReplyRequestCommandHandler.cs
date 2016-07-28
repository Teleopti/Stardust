using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ReplyRequestCommandHandler : IHandleCommand<ReplyRequestCommand>
	{
		private readonly IPersonRequestRepository _personRequestRepository;

		public ReplyRequestCommandHandler(IPersonRequestRepository personRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
		}

		public void Handle(ReplyRequestCommand command)
		{
			command.ErrorMessages = new List<string>();
			if (string.IsNullOrEmpty(command.Message))
			{
				return;
			}
			var personRequest = _personRequestRepository.Get(command.PersonRequestId);
			if (personRequest == null)
			{
				return;
			}
			if (!command.Message.IsNullOrEmpty())
			{
				if (tryReplyMessage(personRequest, command))
				{
					command.AffectedRequestId = command.PersonRequestId;
					command.IsReplySuccess = true;
				}
			}
		}

		private bool tryReplyMessage(IPersonRequest personRequest,ReplyRequestCommand command)
		{
			try
			{
				if (!personRequest.CheckReplyTextLength(command.Message))
				{
					command.ErrorMessages.Add(UserTexts.Resources.RequestInvalidMessageLength);
					return false;
				}
				personRequest.Reply(command.Message);
			}
			catch (InvalidOperationException)
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidMessageModification, personRequest.StatusText));
				return false;
			}
			return true;
		}
	}
}
