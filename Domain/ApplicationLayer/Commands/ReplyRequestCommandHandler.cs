using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

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
			if (string.IsNullOrEmpty(command.ReplyMessage))
			{
				return;
			}
			var personRequest = _personRequestRepository.Get(command.PersonRequestId);
			if (personRequest == null)
			{
				return;
			}
			command.IsReplySuccess = command.TryReplyMessage(personRequest);
		}
	}
}
