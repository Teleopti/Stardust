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

			var personRequest = _personRequestRepository.Get(command.PersonRequestId);

			if (personRequest == null)
			{
				return;
			}

			if (personRequest.Reply(command.Message))
			{
				command.AffectedRequestId = command.PersonRequestId;
				return;
			}

			foreach (var errorMessage in command.ErrorMessages)
			{
				command.ErrorMessages.Add(errorMessage);
			}
		}
	}
}
