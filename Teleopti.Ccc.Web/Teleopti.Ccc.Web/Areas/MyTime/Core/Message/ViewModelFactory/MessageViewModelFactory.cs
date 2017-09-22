using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.ViewModelFactory
{
    public class MessageViewModelFactory : IMessageViewModelFactory
    {
        private readonly IPushMessageProvider _messageProvider;
	    private readonly IPersonNameProvider _personNameProvider;
	    private readonly IUserTimeZone _userTimeZone;

	    public MessageViewModelFactory(IPushMessageProvider messageProvider, IPersonNameProvider personNameProvider, IUserTimeZone userTimeZone)
	    {
		    _messageProvider = messageProvider;
		    _personNameProvider = personNameProvider;
		    _userTimeZone = userTimeZone;
	    }

        public IList<MessageViewModel> CreatePageViewModel(Paging paging)
        {
            var messages = _messageProvider.GetMessages(paging);

            return messages.Select(transform).ToList();
        }

    	public MessagesInformationViewModel CreateMessagesInformationViewModel(Guid messageId)
    	{
			return new MessagesInformationViewModel
			       	{
						UnreadMessagesCount = _messageProvider.UnreadMessageCount,
						MessageItem = transform(_messageProvider.GetMessage(messageId))
			       	};
    	}

	    private MessageViewModel transform(IPushMessageDialogue pushMessageDialogue)
	    {
			return new MessageViewModel
			{
				MessageType = (int)pushMessageDialogue.PushMessage.MessageType,
				MessageId = pushMessageDialogue.Id.ToString(),
				Title = pushMessageDialogue.PushMessage.GetTitle(new NoFormatting()),
				Message = pushMessageDialogue.Message(new NoFormatting()),
				Sender = pushMessageDialogue.PushMessage.Sender == null ? null : _personNameProvider.BuildNameFromSetting(pushMessageDialogue.PushMessage.Sender.Name),
				Date =
					pushMessageDialogue.UpdatedOn.HasValue
						? TimeZoneInfo.ConvertTimeFromUtc(pushMessageDialogue.UpdatedOn.Value, _userTimeZone.TimeZone())
						: (DateTime?)null,
				IsRead = pushMessageDialogue.IsReplied,
				AllowDialogueReply = pushMessageDialogue.PushMessage.AllowDialogueReply,
				DialogueMessages = pushMessageDialogue.DialogueMessages.Select(d => new DialogueMessageViewModel { Created = TimeZoneInfo.ConvertTimeFromUtc(d.Created, _userTimeZone.TimeZone()).ToShortDateTimeString(), Text = d.Text, Sender = _personNameProvider.BuildNameFromSetting(d.Sender.Name), SenderId = d.Sender.Id.GetValueOrDefault() }).ToArray(),
				ReplyOptions = pushMessageDialogue.PushMessage.ReplyOptions.ToArray()
			};
		}
    }
}