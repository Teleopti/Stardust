using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider
{
	public class PushMessageDialoguePersister : IPushMessageDialoguePersister
	{
		private readonly IPushMessageDialogueRepository _pushMessageDialogueRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonNameProvider _personNameProvider;
		private readonly IUserTimeZone _userTimeZone;

		public PushMessageDialoguePersister(IPushMessageDialogueRepository pushMessageDialogueRepository,ILoggedOnUser loggedOnUser,IPersonNameProvider personNameProvider, IUserTimeZone userTimeZone)
		{
			_pushMessageDialogueRepository = pushMessageDialogueRepository;
			_loggedOnUser = loggedOnUser;
			_personNameProvider = personNameProvider;
			_userTimeZone = userTimeZone;
		}

		public MessageViewModel PersistMessage(ConfirmMessageViewModel confirmMessage)
		{
			var pushMessageDialogue = _pushMessageDialogueRepository.Get(confirmMessage.Id);
			if(!string.IsNullOrEmpty(confirmMessage.Reply))
			{
				pushMessageDialogue.DialogueReply(confirmMessage.Reply, _loggedOnUser.CurrentUser());
			}
				
			pushMessageDialogue.SetReply(confirmMessage.ReplyOption);

			return new MessageViewModel
			{
				MessageType = (int) pushMessageDialogue.PushMessage.MessageType,
				MessageId = pushMessageDialogue.Id.ToString(),
				Title = pushMessageDialogue.PushMessage.GetTitle(new NoFormatting()),
				Message = pushMessageDialogue.Message(new NoFormatting()),
				Sender = pushMessageDialogue.PushMessage.Sender == null ? null : _personNameProvider.BuildNameFromSetting(pushMessageDialogue.PushMessage.Sender.Name),
				Date =
					pushMessageDialogue.UpdatedOn.HasValue
						? TimeZoneInfo.ConvertTimeFromUtc(pushMessageDialogue.UpdatedOn.Value, _userTimeZone.TimeZone())
						: (DateTime?) null,
				IsRead = pushMessageDialogue.IsReplied,
				AllowDialogueReply = pushMessageDialogue.PushMessage.AllowDialogueReply,
				DialogueMessages = pushMessageDialogue.DialogueMessages.Select(d => new DialogueMessageViewModel {Created = TimeZoneInfo.ConvertTimeFromUtc(d.Created,_userTimeZone.TimeZone()).ToShortDateTimeString() ,Text = d.Text,Sender = _personNameProvider.BuildNameFromSetting(d.Sender.Name),SenderId = d.Sender.Id.GetValueOrDefault()}).ToArray(),
				ReplyOptions = pushMessageDialogue.PushMessage.ReplyOptions.ToArray()
			};
		}
	}
}