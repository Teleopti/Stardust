using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class SendPushMessageToPersonCommandHandler : IHandleCommand<SendPushMessageToPeopleCommandDto>
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPushMessagePersister _pushMessagePersister;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public SendPushMessageToPersonCommandHandler(IPersonRepository personRepository, IPushMessagePersister pushMessagePersister, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_personRepository = personRepository;
			_pushMessagePersister = pushMessagePersister;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Handle(SendPushMessageToPeopleCommandDto command)
		{
			command.Recipients.VerifyCountLessThan(50, "A maximum of 50 recipients is allowed. You tried to send to {0}.");
			var result = new CommandResultDto();
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var people = _personRepository.FindPeople(command.Recipients).ToList();
				if (people.Count > 0)
				{
					people.VerifyCanBeModifiedByCurrentUser(DateOnly.Today);
					
					makeSureAtLeastDefaultReplyOptionExists(command.ReplyOptions);

					var service = SendPushMessageService.CreateConversation(command.Title, command.Message, command.AllowReply).To(people).
						AddReplyOption(command.ReplyOptions);
					service.SendConversation(_pushMessagePersister);
					uow.PersistAll();

					result.AffectedItems = 1;
					result.AffectedId = service.PushMessage.Id;
				}
			}
			command.Result = result;
		}

		private static void makeSureAtLeastDefaultReplyOptionExists(ICollection<string> replyOptions)
		{
			if (replyOptions.Count == 0)
			{
				replyOptions.Add("OK");
			}
		}
	}
}