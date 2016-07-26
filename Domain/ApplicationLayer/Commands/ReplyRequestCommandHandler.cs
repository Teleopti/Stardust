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
			if (tryReplyMessage(personRequest,command))
			{
				command.AffectedRequestId = command.PersonRequestId;
			}
		}

		private bool tryReplyMessage(IPersonRequest personRequest,ReplyRequestCommand command)
		{
			if (!checkMessageLength(personRequest, command))
			{
				return false;
			}
			try
			{
				personRequest.Reply(command.Message);
			}
			catch (InvalidOperationException)
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidMessageModification, personRequest.StatusText));
				return false;
			}
			return true;
		}

		private bool checkMessageLength(IPersonRequest personRequest, ReplyRequestCommand command)
		{
			var orignalMessageLength = personRequest.GetMessage(new NoFormatting()).IsNullOrEmpty()
				? 0
				: personRequest.GetMessage(new NoFormatting()).Length;
			if ((orignalMessageLength + command.Message.Length) > 2000)
			{
				command.ErrorMessages.Add(UserTexts.Resources.RequestInvalidMessageLength);
				return false;
			}
			return true;
		}
	}
}
