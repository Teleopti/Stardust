using System;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Message.Mapping
{
    public class MessageViewModelMapper
    {
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPersonNameProvider _personNameProvider;

	    public MessageViewModelMapper(IUserTimeZone timeZone, IPersonNameProvider personNameProvider)
	    {
		    _userTimeZone = timeZone;
		    _personNameProvider = personNameProvider;
	    }

	    public MessageViewModel Map(IPushMessageDialogue m)
	    {
		    return new MessageViewModel
		    {
				MessageType = (int)m.PushMessage.MessageType,
				MessageId = m.Id.ToString(),
				Title = m.PushMessage.GetTitle(new NoFormatting()),
				Message = m.Message(new NoFormatting()),
				Sender = _personNameProvider.BuildNameFromSetting(m.PushMessage.Sender.Name),
				Date = m.UpdatedOn.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(m.UpdatedOn.Value, _userTimeZone.TimeZone()) : (DateTime?)null,
				IsRead = m.IsReplied,
				AllowDialogueReply = m.PushMessage.AllowDialogueReply,
				DialogueMessages = m.DialogueMessages.Select(map).ToArray(),
				ReplyOptions = m.PushMessage.ReplyOptions.ToArray()
		    };
	    }

	    private DialogueMessageViewModel map(IDialogueMessage m)
	    {
		    return new DialogueMessageViewModel
		    {
			    Text = m.Text,
			    SenderId = m.Sender.Id.GetValueOrDefault(),
			    Sender = _personNameProvider.BuildNameFromSetting(m.Sender.Name),
			    Created = TimeZoneInfo.ConvertTimeFromUtc(m.Created, _userTimeZone.TimeZone()).ToShortDateTimeString()
		    };
	    }
    }
}