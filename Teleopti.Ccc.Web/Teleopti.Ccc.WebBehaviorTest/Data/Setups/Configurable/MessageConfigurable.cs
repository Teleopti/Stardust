using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
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

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var replyOptions = new List<string>() {ReplyOption1, ReplyOption2, ReplyOption3};
			var addOptions = new List<string>();
			foreach (var replyOption in replyOptions)
			{
				if (!string.IsNullOrEmpty(replyOption))
					addOptions.Add(replyOption);
			}
			var conversation =
			SendPushMessageService.CreateConversation(Title, Message, TextReplyAllowed).To(user).From(user).AddReplyOption(addOptions);
			conversation.SendConversation(new PushMessageRepository(currentUnitOfWork), new PushMessageDialogueRepository(currentUnitOfWork));

			if(MyReply!=string.Empty)
			{
				currentUnitOfWork.Current().PersistAll();
				var repository = new PushMessageDialogueRepository(currentUnitOfWork);
				var messageDialogue = repository.LoadAll().First(t => t.PushMessage.GetTitle(new NoFormatting()).Equals(Title));
				messageDialogue.DialogueReply(MyReply,user);
				if (SendersReply != string.Empty) messageDialogue.DialogueReply(SendersReply, user);
			}
		}
	}
}