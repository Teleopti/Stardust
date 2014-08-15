using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			verifyNotTooManyReceivers(command.Recipients);

			var result = new CommandResultDto();
			using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var people = _personRepository.FindPeople(command.Recipients).ToList();
				if (people.Count > 0)
				{
						checkIfAuthorized(people, DateOnly.Today);
					
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

		private static void verifyNotTooManyReceivers(ICollection<Guid> receivers)
		{
			if (receivers.Count > 50)
			{
				throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "A maximum of 50 recipients is allowed. You tried to send to {0}.", receivers.Count));
			}
		}

		private static void makeSureAtLeastDefaultReplyOptionExists(ICollection<string> replyOptions)
		{
			if (replyOptions.Count == 0)
			{
				replyOptions.Add("OK");
			}
		}

		private static void checkIfAuthorized(IEnumerable<IPerson> people, DateOnly dateOnly)
		{
			var authorizationInstance = PrincipalAuthorization.Instance();
			foreach (var person in people)
			{
				if (!authorizationInstance.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, dateOnly, person))
				{
					throw new FaultException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "You're not allowed to work with this person ({0}).", person.Name));
				}
			}
		}
	}
}