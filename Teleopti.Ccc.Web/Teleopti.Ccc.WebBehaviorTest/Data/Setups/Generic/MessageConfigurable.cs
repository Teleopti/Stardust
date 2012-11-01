using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class MessageConfigurable : IUserDataSetup
	{
		public string Title { get; set; }
		public string Message { get; set; }
		public bool IsOldestMessage { get; set; }
		public bool TextReplyAllowed { get; set; }
		public string MyReply { get; set; }
		public string SendersReply { get; set; }
		public string ReplyOption1 { get; set; }
		public string ReplyOption2 { get; set; }
		public string ReplyOption3 { get; set; }
		

		public MessageConfigurable()
		{
			Message = "Hello";
			TextReplyAllowed = false;
			MyReply = string.Empty;
			SendersReply = string.Empty;
			ReplyOption1 = "OK";
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var sender = new Person();
			var replyOptions = new List<string>() {ReplyOption1, ReplyOption2, ReplyOption3};
			var addOptions = new List<string>();
			foreach (var replyOption in replyOptions)
			{
				if (replyOption != string.Empty)
					addOptions.Add(replyOption);
			}
			var conversation =
			SendPushMessageService.CreateConversation(Title, Message, TextReplyAllowed).To(user).From(sender).AddReplyOption(addOptions);
			conversation.SendConversation(new PushMessageRepository(uow), new PushMessageDialogueRepository(uow));

			if(MyReply!=string.Empty)
			{
				uow.PersistAll();
				var repository = new PushMessageDialogueRepository(uow);
				var messageDialogue = repository.LoadAll().First(t => t.PushMessage.GetTitle(new NoFormatting()).Equals(Title));
				messageDialogue.DialogueReply(MyReply,user);
				if (SendersReply != string.Empty) messageDialogue.DialogueReply(SendersReply, sender);
			}

			if (IsOldestMessage)
				Thread.Sleep(1010);
		}
	}
}