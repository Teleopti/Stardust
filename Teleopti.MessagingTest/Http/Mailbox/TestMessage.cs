using System;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	public class TestMessage : Message
	{
		public TestMessage()
		{
			StartDate = Subscription.DateToString(DateTime.UtcNow);
			EndDate = Subscription.DateToString(DateTime.UtcNow);
		}
	}
}