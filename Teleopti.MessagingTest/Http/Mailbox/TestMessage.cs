using System;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	public class testMessage : Message
	{
		public testMessage()
		{
			StartDate = Subscription.DateToString(DateTime.UtcNow);
			EndDate = Subscription.DateToString(DateTime.UtcNow);
		}
	}
}