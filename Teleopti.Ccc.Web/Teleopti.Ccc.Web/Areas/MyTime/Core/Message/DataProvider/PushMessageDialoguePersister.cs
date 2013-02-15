using AutoMapper;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider
{
	public class PushMessageDialoguePersister : IPushMessageDialoguePersister
	{
		private readonly IPushMessageDialogueRepository _pushMessageDialogueRepository;
		private readonly IMappingEngine _mapper;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPushMessageRepository _pushMessageRepository;

		public PushMessageDialoguePersister(IPushMessageDialogueRepository pushMessageDialogueRepository, IMappingEngine mapper, ILoggedOnUser loggedOnUser,IPushMessageRepository pushMessageRepository)
		{
			_pushMessageDialogueRepository = pushMessageDialogueRepository;
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
			_pushMessageRepository = pushMessageRepository;
		}

		public MessageViewModel PersistMessage(ConfirmMessageViewModel confirmMessage)
		{
			var pushMessageDialogue = _pushMessageDialogueRepository.Get(confirmMessage.Id);
			if(!string.IsNullOrEmpty(confirmMessage.Reply))
			{
				pushMessageDialogue.DialogueReply(confirmMessage.Reply, _loggedOnUser.CurrentUser());
			}
				
			pushMessageDialogue.SetReply(confirmMessage.ReplyOption);

			return _mapper.Map<IPushMessageDialogue, MessageViewModel>(pushMessageDialogue);
		}

		public void SendNewPushMessageToLoggedOnUser(string title, string message)
		{
			SendPushMessageService.CreateConversation(title, message, false).To(_loggedOnUser.CurrentUser()).From(_loggedOnUser.CurrentUser()).AddReplyOption("OK").SendConversation(_pushMessageRepository,_pushMessageDialogueRepository);
		}
	}
}