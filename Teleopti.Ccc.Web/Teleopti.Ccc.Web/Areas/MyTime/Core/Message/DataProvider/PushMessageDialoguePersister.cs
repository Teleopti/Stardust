using System;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider
{
	public class PushMessageDialoguePersister : IPushMessageDialoguePersister
	{
		private readonly IPushMessageDialogueRepository _pushMessageDialogueRepository;
		private readonly IMappingEngine _mapper;
		private readonly ILoggedOnUser _loggedOnUser;

		public PushMessageDialoguePersister(IPushMessageDialogueRepository pushMessageDialogueRepository, IMappingEngine mapper, ILoggedOnUser loggedOnUser)
		{
			_pushMessageDialogueRepository = pushMessageDialogueRepository;
			_mapper = mapper;
			_loggedOnUser = loggedOnUser;
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
	}
}