using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	[TestFixture]
	[MessagingTest]
	public class EmptyConnectionStringTest
	{
		public IMessageListener Target;
		public FakeUrl Url;
		public MessageBrokerServerBridge Server;
		
		[Test]
		public void ShouldNotCreatMailboxWhenEmptyConnectionString()
		{
			Url.Is("");

			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);

			Server.Requests.Should().Be.Empty();
		}
	}
}
