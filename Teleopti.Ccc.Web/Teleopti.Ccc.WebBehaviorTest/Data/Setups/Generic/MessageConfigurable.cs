using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Common.Messaging;
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

		public MessageConfigurable()
		{
			Message = "Hello";
			TextReplyAllowed = false;
			MyReply = string.Empty;
			SendersReply = string.Empty;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var conversation = SendPushMessageService.CreateConversation(Title, Message, TextReplyAllowed).To(user).From(user).AddReplyOption("OK");
			conversation.SendConversation(new PushMessageRepository(uow), new PushMessageDialogueRepository(uow));

			if (IsOldestMessage)
				Thread.Sleep(1010);
		}
	}
}