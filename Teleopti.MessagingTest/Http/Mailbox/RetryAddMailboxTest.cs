using System;
using System.Net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.MessagingTest.Http.Mailbox
{
	[TestFixture]
	[MessagingTest]
	public class RetryAddMailboxTest
	{
		public IMessageListener Target;
		public MessageBrokerServerBridge Server;
		public FakeTime Time;
		public ISystemCheck SystemCheck;

		[Test]
		public void ShouldRetryIfServerFails()
		{
			var wasEventHandlerCalled = false;
			Server.GivesError(HttpStatusCode.ServiceUnavailable);
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());
			Server.IsHappy();
			Time.Passes("60".Seconds());

			Server.Receives(new TestMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Time.Passes("120".Seconds());

			wasEventHandlerCalled.Should().Be.True();
		}

		[Test]
		public void ShouldRetryIfNoConnection()
		{
			var wasEventHandlerCalled = false;
			Server.Down();
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());
			Server.IsHappy();

			Time.Passes("60".Seconds());
			Server.Receives(new TestMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Time.Passes("120".Seconds());

			wasEventHandlerCalled.Should().Be.True();
		}

		[Test]
		public void ShouldRetryIfServerIsSlow()
		{
			var wasEventHandlerCalled = false;
			Server.IsSlow();
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => wasEventHandlerCalled = true, typeof(ITestType), false, true);
			Time.Passes("60".Seconds());
			Server.IsHappy();
			Time.Passes("60".Seconds());

			Server.Receives(new TestMessage
			{
				BusinessUnitId = Guid.Empty.ToString(),
				DataSource = string.Empty,
				DomainQualifiedType = "ITestType",
				DomainType = "ITestType",
			});
			Time.Passes("120".Seconds());

			wasEventHandlerCalled.Should().Be.True();
		}

		[Test]
		public void ShouldNotRetryOthersIfOneFails()
		{
			Server.IsSlow();
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Target.RegisterSubscription(string.Empty, Guid.Empty, (sender, args) => { }, typeof(ITestType), false, true);
			Server.Requests.Clear();

            Time.Passes("60".Seconds());

			Server.Requests.Should().Have.Count.EqualTo(1);
		}
	}
}