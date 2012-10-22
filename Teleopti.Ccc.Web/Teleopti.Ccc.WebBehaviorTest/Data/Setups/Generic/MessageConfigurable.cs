using System.Collections.Generic;
using System.Globalization;
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

		public MessageConfigurable()
		{
			Message = "Hello";
			TextReplyAllowed = false;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var message = new PushMessage(new[] {"OK"})
							  {
								  Title = Title,
								  Message = Message,
								  AllowDialogueReply = TextReplyAllowed
							  };

			var repository = new PushMessageRepository(uow);
			repository.Add(message, new List<IPerson> {user});

			if (IsOldestMessage)
				Thread.Sleep(1010);
		}
	}
}